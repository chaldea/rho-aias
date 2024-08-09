using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias;

internal class UdpForwarder : ForwarderBase
{
    private readonly ILogger<TcpForwarder> _logger;
    private UdpClient _listenSocket;
    private bool _shutdown = false;
    private readonly ConcurrentDictionary<IPEndPoint, Channel<byte[]>> _clients = new();
    private CancellationTokenSource _cts;

    public UdpForwarder(
        ILogger<TcpForwarder> logger,
        IServiceProvider service) : base(service)
    {
        _logger = logger;
    }

    public override void Register(Proxy proxy)
    {
        base.Register(proxy);
        var localEndPoint = new IPEndPoint(IPAddress.Any, proxy.RemotePort);
        _listenSocket = new UdpClient(localEndPoint);
        _shutdown = false;
        _logger.LogInformation($"Register udp forwarder {IPAddress.Any}:{proxy.RemotePort} => {proxy.LocalIP}:{proxy.LocalPort}");
        _cts = new CancellationTokenSource();
        Receive(_cts.Token);
    }

    public override void UnRegister()
    {
        _logger.LogInformation($"UnRegister udp forwarder {IPAddress.Any}:{_proxy.RemotePort} => {_proxy.LocalIP}:{_proxy.LocalPort}");
        if (_shutdown)
            return;
        _cts.Cancel(false);
        _listenSocket.Close();
        _clients.Clear();
        _shutdown = true;
    }

    private void Receive(CancellationToken cancellation)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                if (cancellation.IsCancellationRequested) break;
                var recv = await _listenSocket.ReceiveAsync(cancellation);
                if (!_clients.TryGetValue(recv.RemoteEndPoint, out var channel))
                {
                    _clients[recv.RemoteEndPoint] = channel = Channel.CreateUnbounded<byte[]>();
                    Dispatch(recv.RemoteEndPoint, channel, cancellation);
                }
                await channel.Writer.WriteAsync(recv.Buffer, cancellation);
            }
        });
    }

    private void Dispatch(IPEndPoint remoteEndPoint, Channel<byte[]> channel, CancellationToken cancellation)
    {
        Task.Run(async () =>
        {
            using (var stream1 = await CreateAsync(cancellation))
            using (var stream2 = _listenSocket.GetStream(remoteEndPoint, channel))
            {
                var taskX = stream1.CopyToAsync(stream2, cancellation);
                var taskY = stream2.CopyToAsync(stream1, cancellation);
                await Task.WhenAny(taskX, taskY);
            }
        });
    }
}
