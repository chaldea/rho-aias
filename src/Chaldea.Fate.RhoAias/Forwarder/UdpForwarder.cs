using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias;

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
