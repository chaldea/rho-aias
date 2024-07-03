using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Connections.Features;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Yarp.ReverseProxy.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Buffers;

namespace Chaldea.Fate.RhoAias;

internal interface IForwarder
{
    Proxy Proxy { get; }
    void Register(Proxy proxy);
    void UnRegister();
    ValueTask<Stream> CreateAsync(CancellationToken cancellation);
    Task ForwardAsync(string requestId, IConnectionLifetimeFeature lifetime, IConnectionTransportFeature transport);
}

abstract class ForwarderBase : IForwarder
{
    protected Proxy _proxy;
    protected readonly ConcurrentDictionary<string, (TaskCompletionSource<Stream>, CancellationToken)> ForwarderTasks = new();
    private readonly ILogger<ForwarderBase> _logger;
    private readonly ICompressor _compressor;

    public Proxy Proxy => _proxy;

    protected ForwarderBase(IServiceProvider service)
    {
        _logger = service.GetRequiredService<ILogger<ForwarderBase>>();
        _compressor = service.GetRequiredService<ICompressor>();
    }

    public virtual void Register(Proxy proxy)
    {
        _proxy = proxy;
    }

    public abstract void UnRegister();

    public virtual ValueTask<Stream> CreateAsync(CancellationToken cancellation)
    {
        return new ValueTask<Stream>(Stream.Null);
    }

    public virtual async Task ForwardAsync(string requestId, IConnectionLifetimeFeature lifetime, IConnectionTransportFeature transport)
    {
        if (!ForwarderTasks.TryRemove(requestId, out var responseAwaiter))
        {
            return;
        }

        try
        {
            _logger.LogInformation($"Forward Starting: {requestId}");
            var compressor = _proxy.Compressed ? _compressor : null;
            using var reverseConnection = new WebSocketStream(lifetime, transport, compressor);
            responseAwaiter.Item1.TrySetResult(reverseConnection);
            CancellationTokenSource cts;
            if (responseAwaiter.Item2 != CancellationToken.None)
            {
                cts = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ConnectionClosed,
                    responseAwaiter.Item2);
            }
            else
            {
                cts = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ConnectionClosed);
            }

            var closedAwaiter = new TaskCompletionSource<object>();
            await closedAwaiter.Task.WaitAsync(cts.Token);
            _logger.LogInformation($"Forward Closed: {requestId}");
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "");
        }
    }
}

internal class HttpForwarder : ForwarderBase
{
    private readonly ILogger<ForwarderManager> _logger;
    private readonly IHubContext<ClientHub> _hub;
    private readonly IProxyConfigProvider _proxyConfigProvider;

    public HttpForwarder(
        ILogger<ForwarderManager> logger,
        IHubContext<ClientHub> hub,
        IProxyConfigProvider proxyConfigProvider,
        IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _logger = logger;
        _hub = hub;
        _proxyConfigProvider = proxyConfigProvider;
    }

    public override void Register(Proxy proxy)
    {
        base.Register(proxy);
        _logger.LogInformation($"Register http forwarder {proxy.GetHosts()} => {proxy.GetUrl()}");
        var config = _proxyConfigProvider.GetConfig();
        var routes = config.Routes.ToList();
        var clusters = config.Clusters.ToList();
        routes.RemoveAll(x => x.ClusterId == proxy.Name);
        clusters.RemoveAll(x => x.ClusterId == proxy.Name);
        if (proxy is { RouteConfig: not null, ClusterConfig: not null })
        {
            var route = JsonSerializer.Deserialize<RouteConfig>(proxy.RouteConfig);
            var cluster = JsonSerializer.Deserialize<ClusterConfig>(proxy.ClusterConfig);
            if (route != null) routes.Add(route);
            if (cluster != null) clusters.Add(cluster);
        }
        else
        {
            routes.Add(new RouteConfig
            {
                ClusterId = proxy.Name,
                RouteId = proxy.Name,
                Match = new RouteMatch { Path = proxy.Path, Hosts = proxy.Hosts }
            });
            clusters.Add(new ClusterConfig
            {
                ClusterId = proxy.Name,
                Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                {
                    { "destination", new DestinationConfig() { Address = $"{proxy.GetUrl()}" } }
                }
            });
        }

        (_proxyConfigProvider as InMemoryConfigProvider).Update(routes, clusters);
    }

    public override void UnRegister()
    {
        _logger.LogInformation($"UnRegister http forwarder {_proxy.GetHosts()} => {_proxy.GetUrl()}");
        var config = _proxyConfigProvider.GetConfig();
        var routes = config.Routes.ToList();
        var clusters = config.Clusters.ToList();
        routes.RemoveAll(x => x.RouteId == _proxy.Name);
        clusters.RemoveAll(x => x.ClusterId == _proxy.Name);
        (_proxyConfigProvider as InMemoryConfigProvider).Update(routes, clusters);
    }

    public override async ValueTask<Stream> CreateAsync(CancellationToken cancellation)
    {
        var requestId = Guid.NewGuid().ToString().Replace("-", "");
        TaskCompletionSource<Stream> tcs = new();
        cancellation.Register(() =>
        {
            _logger.LogInformation($"Web Forward TimeOut:{requestId}");
            tcs.TrySetCanceled();
        });
        ForwarderTasks.TryAdd(requestId, (tcs, cancellation));
        await _hub.Clients
            .Client(_proxy.Client.ConnectionId)
            .SendAsync("CreateForwarder", requestId, _proxy, cancellationToken: cancellation);
        return await tcs.Task.WaitAsync(cancellation);
    }

}

internal class TcpForwarder : ForwarderBase
{
    private Socket _listenSocket;
    private bool _shutdown = false;
    private readonly ILogger<TcpForwarder> _logger;
    private readonly IHubContext<ClientHub> _hub;

    public TcpForwarder(
        ILogger<TcpForwarder> logger,
        IHubContext<ClientHub> hub,
        IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _logger = logger;
        _hub = hub;
    }

    public override void Register(Proxy proxy)
    {
        base.Register(proxy);
        var localEndPoint = new IPEndPoint(IPAddress.Any, proxy.RemotePort);
        _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _listenSocket.Bind(localEndPoint);
        _shutdown = false;
        _logger.LogInformation($"Register tcp forwarder {IPAddress.Any}:{proxy.RemotePort} => {proxy.LocalIP}:{proxy.LocalPort}");
        _listenSocket.Listen();
        Accept(null);
    }

    public override void UnRegister()
    {
        _logger.LogInformation($"UnRegister tcp forwarder {IPAddress.Any}:{_proxy.RemotePort} => {_proxy.LocalIP}:{_proxy.LocalPort}");
        Stop();
    }

    private void Accept(SocketAsyncEventArgs? acceptEventArg)
    {
        if (acceptEventArg == null)
        {
            acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += AcceptEventArg_Completed;
        }
        else
        {
            acceptEventArg.AcceptSocket = null;
        }

        var willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);
        if (!willRaiseEvent)
        {
            ProcessAcceptAsync(acceptEventArg);
        }
    }

    private void AcceptEventArg_Completed(object? sender, SocketAsyncEventArgs e)
    {
        ProcessAcceptAsync(e);
    }

    private void ProcessAcceptAsync(SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            Dispatch(e.AcceptSocket, CancellationToken.None);
            Accept(e);
        }
        else
        {
            Stop();
        }
    }

    private void Stop()
    {
        if (_shutdown)
            return;

        try
        {
            if (_listenSocket.Connected)
            {
                _listenSocket.Shutdown(SocketShutdown.Both);
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            _shutdown = true;
            _listenSocket.Close();
        }
    }

    private void Dispatch(Socket socket, CancellationToken cancellation)
    {
        Task.Run(async () =>
        {
            var requestId = Guid.NewGuid().ToString().Replace("-", "");
            await Task.Yield();
            var tcs = new TaskCompletionSource<Stream>();
            ForwarderTasks.TryAdd(requestId, (tcs, cancellation));
            await _hub.Clients
                .Client(_proxy.Client.ConnectionId)
                .SendAsync("CreateForwarder", requestId, _proxy, cancellationToken: cancellation);
            using (var stream1 = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10)))
            using (var stream2 = new NetworkStream(socket, true) { ReadTimeout = 1000 * 60 * 10 })
            {
                await Task.WhenAny(stream1.CopyToAsync(stream2), stream2.CopyToAsync(stream1));
            }
        });
    }
}

internal class UdpForwarder : ForwarderBase
{
    private readonly ILogger<TcpForwarder> _logger;
    private readonly IHubContext<ClientHub> _hub;
    private UdpClient _listenSocket;
    private bool _shutdown = false;

    public UdpForwarder(
        ILogger<TcpForwarder> logger,
        IHubContext<ClientHub> hub,
        IServiceProvider service) : base(service)
    {
        _logger = logger;
        _hub = hub;
    }

    public override void Register(Proxy proxy)
    {
        base.Register(proxy);
        var localEndPoint = new IPEndPoint(IPAddress.Any, proxy.RemotePort);
        _listenSocket = new UdpClient(localEndPoint);
        _shutdown = false;
        _logger.LogInformation($"Register udp forwarder {IPAddress.Any}:{proxy.RemotePort} => {proxy.LocalIP}:{proxy.LocalPort}");
        Receive(CancellationToken.None);
    }

    public override void UnRegister()
    {
        _logger.LogInformation($"UnRegister udp forwarder {IPAddress.Any}:{_proxy.RemotePort} => {_proxy.LocalIP}:{_proxy.LocalPort}");
        if (_shutdown)
            return;
        _listenSocket.Close();
        _shutdown = true;
    }

    private void Receive(CancellationToken cancellation)
    {
        Task.Run(async() =>
        {
            var requestId = Guid.NewGuid().ToString().Replace("-", "");
            await Task.Yield();
            var tcs = new TaskCompletionSource<Stream>();
            ForwarderTasks.TryAdd(requestId, (tcs, cancellation));
            await _hub.Clients
                .Client(_proxy.Client.ConnectionId)
                .SendAsync("CreateForwarder", requestId, _proxy, cancellationToken: cancellation);

            using (var stream1 = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10), cancellation))
            using (var stream2 = _listenSocket.GetStream(true))
            {
                await Task.WhenAny(stream1.CopyToAsync(stream2, cancellation), stream2.CopyToAsync(stream1, cancellation));
            }
        });
    }
}

internal sealed class WebSocketStream : Stream
{
    private readonly Stream readStream;
    private readonly Stream wirteStream;
    private readonly IConnectionLifetimeFeature lifetimeFeature;

    public WebSocketStream(IConnectionLifetimeFeature lifetimeFeature, IConnectionTransportFeature transportFeature, ICompressor? compressor)
    {
        var input = transportFeature.Transport.Input.AsStream();
        var output = transportFeature.Transport.Output.AsStream();
        if (compressor != null)
        {
            this.readStream = compressor.Decompress(input);
            this.wirteStream = compressor.Compress(output);
        }
        else
        {
            this.readStream = input;
            this.wirteStream = output;
        }

        this.lifetimeFeature = lifetimeFeature;
    }

    public WebSocketStream(Stream stream)
    {
        this.readStream = stream;
        this.wirteStream = stream;
        this.lifetimeFeature = null;
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush()
    {
        this.wirteStream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return this.wirteStream.FlushAsync(cancellationToken);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return this.readStream.Read(buffer, offset, count);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return this.readStream.ReadAsync(buffer, cancellationToken);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return this.readStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        this.wirteStream.Write(buffer, offset, count);
        this.wirteStream.Flush();
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        this.wirteStream.Write(buffer);
        this.wirteStream.Flush();
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await this.wirteStream.WriteAsync(buffer, offset, count, cancellationToken);
        await this.wirteStream.FlushAsync(cancellationToken);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await this.wirteStream.WriteAsync(buffer, cancellationToken);
        await this.wirteStream.FlushAsync(cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        this.lifetimeFeature?.Abort();
    }

    public override ValueTask DisposeAsync()
    {
        this.lifetimeFeature?.Abort();
        return ValueTask.CompletedTask;
    }
}

internal class UdpStream : Stream
{
    private readonly UdpClient _client;
    private readonly bool _remoteWrite;
    private IPEndPoint? _remoteEndPoint;

    public UdpStream(UdpClient client, bool remoteWrite)
    {
        _client = client;
        _remoteWrite = remoteWrite;
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var recv = _client.Receive(ref _remoteEndPoint);
        recv.CopyTo(buffer, 0);
        return recv.Length;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var recv = await _client.ReceiveAsync(cancellationToken);
        _remoteEndPoint = recv.RemoteEndPoint;
        recv.Buffer.CopyTo(buffer, 0);
        return recv.Buffer.Length;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        var recv = await _client.ReceiveAsync(cancellationToken);
        _remoteEndPoint = recv.RemoteEndPoint;
        recv.Buffer.CopyTo(buffer);
        return recv.Buffer.Length;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (CanRemoteWrite)
        {
            _client.Send(buffer, count, _remoteEndPoint);
        }
        else
        {
            _client.Send(buffer, count);
        }
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (CanRemoteWrite)
        {
            await _client.SendAsync(buffer, count, _remoteEndPoint);
        }
        else
        {
            await _client.SendAsync(buffer, count);
        }
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        if (CanRemoteWrite)
        {
            await _client.SendAsync(buffer, _remoteEndPoint, cancellationToken);
        }
        else
        {
            await _client.SendAsync(buffer, cancellationToken);
        }
    }

    public bool CanRemoteWrite => _remoteWrite && _remoteEndPoint != null;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
}

internal static class StreamExtensions
{
    public static UdpStream GetStream(this UdpClient client, bool remoteWrite = false)
    {
        return new UdpStream(client, remoteWrite);
    }

    public static async Task CopyToAsync(this Stream source, Stream destination, bool flush, CancellationToken cancellationToken)
    {
        const int DefaultCopyBufferSize = 81920;
        var bufferSize = DefaultCopyBufferSize;
        if (source.CanSeek)
        {
            var length = source.Length;
            var position = source.Position;
            if (length <= position)
            {
                bufferSize = 1;
            }
            else
            {
                var remaining = length - position;
                if (remaining > 0)
                {
                    bufferSize = (int)Math.Min(bufferSize, remaining);
                }
            }
        }

        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);
        if (!destination.CanWrite)
        {
            if (destination.CanRead)
            {
                throw new Exception();
            }

            throw new Exception();
        }

        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
                if (flush) await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
