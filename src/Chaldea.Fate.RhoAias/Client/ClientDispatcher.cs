using System.Net.Sockets;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias
{
    internal interface IClientDispatcher
    {
        Task<Stream> CreateLocalAsync(string localIp, int port, CancellationToken cancellationToken);
        Task<Stream> CreateRemoteAsync(string serverUrl, string requestId, CancellationToken cancellationToken);
    }

    internal class DispatcherBase : IClientDispatcher
    {
        private readonly ILogger<DispatcherBase> _logger;

        public DispatcherBase(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<DispatcherBase>>();
        }

        public virtual async Task<Stream> CreateLocalAsync(string localIp, int port, CancellationToken cancellationToken)
        {
            var socket = await ConnectAsync(localIp, port);
            return new NetworkStream(socket, true) { ReadTimeout = 1000 * 60 * 10 };
        }

        public virtual async Task<Stream> CreateRemoteAsync(string serverUrl, string requestId, CancellationToken cancellationToken)
        {
            var uri = new Uri(serverUrl);
            var socket = await ConnectAsync(uri.Host, uri.Port);
            var serverStream = new NetworkStream(socket, true) { ReadTimeout = 1000 * 60 * 10 };
            var reverse = $"PROXY /{requestId} HTTP/1.1\r\nHost: {uri.Host}:{uri.Port}\r\n\r\n";
            var requestMsg = Encoding.UTF8.GetBytes(reverse);
            await serverStream.WriteAsync(requestMsg, cancellationToken);
            return serverStream;
        }

        protected virtual async Task<Socket> ConnectAsync(string host, int port)
        {
            _logger.LogInformation($"Create socket: {host}:{port}");
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var dnsEndPoint = new DnsEndPoint(host, port);
            await socket.ConnectAsync(dnsEndPoint);
            return socket;
        }
    }

    internal class TcpDispatcher : DispatcherBase
    {
        public TcpDispatcher(
            IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
        }
    }

    internal class UdpDispatcher : DispatcherBase
    {
        private readonly ILogger<UdpDispatcher> _logger;

        public UdpDispatcher(
            ILogger<UdpDispatcher> logger,
            IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
            _logger = logger;
        }

        public override Task<Stream> CreateLocalAsync(string localIp, int port, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Create socket: {localIp}:{port}");
            var client = new UdpClient();
            client.Connect(localIp, port);
            _logger.LogInformation($"Client EndPoint: {client.Client.LocalEndPoint}");
            Stream stream = client.GetStream();
            return Task.FromResult(stream);
        }

        protected override async Task<Socket> ConnectAsync(string host, int port)
        {
            _logger.LogInformation($"Create socket: {host}:{port}");
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.SendBufferSize = 8192;
            socket.ReceiveBufferSize = 8192;
            socket.SendTimeout = 5000;
            socket.ReceiveTimeout = 5000;
            var dnsEndPoint = new DnsEndPoint(host, port);
            await socket.ConnectAsync(dnsEndPoint);
            return socket;
        }
    }
}
