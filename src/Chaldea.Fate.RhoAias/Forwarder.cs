using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Connections.Features;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Microsoft.AspNetCore.SignalR;
using Yarp.ReverseProxy.Configuration;

namespace Chaldea.Fate.RhoAias;

internal interface IForwarder
{
    Proxy Proxy { get; }
    void Register(Proxy proxy);
    void UnRegister();
    ValueTask<Stream> CreateAsync(CancellationToken cancellation);
    Task ForwardAsync(string requestId, IConnectionLifetimeFeature lifetime, IConnectionTransportFeature transport);
}

abstract class ForwarderBase: IForwarder
{
    protected Proxy _proxy;
    protected readonly ConcurrentDictionary<string, (TaskCompletionSource<Stream>, CancellationToken)> ForwarderTasks = new();
    private ILogger<ForwarderBase> _logger;

    public Proxy Proxy => _proxy;

    protected ForwarderBase(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ForwarderBase>();
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
            using var reverseConnection = new WebSocketStream(lifetime, transport);
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

internal class WebForwarder : ForwarderBase
{
    private readonly ILogger<ForwarderManager> _logger;
    private readonly IHubContext<ClientHub> _hub;
    private readonly IProxyConfigProvider _proxyConfigProvider;

    public WebForwarder(
        ILogger<ForwarderManager> logger,
        IHubContext<ClientHub> hub,
        IProxyConfigProvider proxyConfigProvider,
        ILoggerFactory loggerFactory)
        :base(loggerFactory)
    {
        _logger = logger;
        _hub = hub;
        _proxyConfigProvider = proxyConfigProvider;
    }

    public override void Register(Proxy proxy)
    {
        base.Register(proxy);
        _logger.LogInformation($"Register web forwarder {proxy.GetHosts()} => {proxy.GetUrl()}");
        var config = _proxyConfigProvider.GetConfig();
        var routes = config.Routes.ToList();
        var clusters = config.Clusters.ToList();
        routes.RemoveAll(x => x.RouteId == proxy.Name);
        clusters.RemoveAll(x => x.ClusterId == proxy.Name);
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
                { "destination", new DestinationConfig() {Address = $"{proxy.GetUrl()}"} }
            }
        });
        (_proxyConfigProvider as InMemoryConfigProvider).Update(routes, clusters);
    }

    public override void UnRegister()
    {
        _logger.LogInformation($"UnRegister web forwarder {_proxy.GetHosts()} => {_proxy.GetUrl()}");
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

    private async ValueTask<Stream> OfflinePage(string host, SocketsHttpConnectionContext context)
    {
        var bytes = Encoding.UTF8.GetBytes(
            $"HTTP/1.1 200 OK\r\nContent-Type:text/html; charset=utf-8\r\n\r\nPage Offline\r\n");

        return await Task.FromResult(new ResponseStream(bytes));
    }
}

internal class PortForwarder : ForwarderBase
{
    private Socket _listenSocket;
    private bool _shutdown = false;
    private readonly ILogger<PortForwarder> _logger;
    private readonly IHubContext<ClientHub> _hub;

    public PortForwarder(
        ILogger<PortForwarder> logger, 
        IHubContext<ClientHub> hub, ILoggerFactory loggerFactory)
        :base(loggerFactory)
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
        _logger.LogInformation($"Register port forwarder {IPAddress.Any}:{proxy.RemotePort} => {proxy.LocalIP}:{proxy.LocalPort}");
        _listenSocket.Listen();
        Accept(null);
    }

    public override void UnRegister()
    {
        _logger.LogInformation($"UnRegister port forwarder {IPAddress.Any}:{_proxy.RemotePort} => {_proxy.LocalIP}:{_proxy.LocalPort}");
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

internal sealed class WebSocketStream : Stream
{
    private readonly Stream readStream;
    private readonly Stream wirteStream;
    private readonly IConnectionLifetimeFeature lifetimeFeature;

    public WebSocketStream(IConnectionLifetimeFeature lifetimeFeature, IConnectionTransportFeature transportFeature)
    {
        this.readStream = transportFeature.Transport.Input.AsStream();
        this.wirteStream = transportFeature.Transport.Output.AsStream();
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
    public override void Write(byte[] buffer, int offset, int count)
    {
        this.wirteStream.Write(buffer, offset, count);
    }
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return this.readStream.ReadAsync(buffer, cancellationToken);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return this.readStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        this.wirteStream.Write(buffer);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return this.wirteStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await this.wirteStream.WriteAsync(buffer, cancellationToken);
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

internal sealed class ResponseStream : Stream
{
    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    readonly MemoryStream m_Stream;

    public ResponseStream(byte[] bytes)
    {
        m_Stream = new MemoryStream(bytes);
    }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    bool complete;

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (!complete)
        {
            return 0;
        };

        var len = m_Stream.Read(buffer, offset, count);
        return len;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        Console.Write(Encoding.UTF8.GetString(buffer, offset, count));
        complete = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        m_Stream.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        Dispose(true);
        return ValueTask.CompletedTask;
    }
}
