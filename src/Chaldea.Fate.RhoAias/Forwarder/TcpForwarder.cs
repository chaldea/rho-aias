using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias;

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
            
            using (var stream1 = await CreateAsync(cancellation))
            using (var stream2 = new NetworkStream(socket, true) { ReadTimeout = 1000 * 60 * 10 })
            {
                await Task.WhenAny(stream1.CopyToAsync(stream2), stream2.CopyToAsync(stream1));
            }
        });
    }
}
