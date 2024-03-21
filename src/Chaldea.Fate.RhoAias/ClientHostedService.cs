using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Chaldea.Fate.RhoAias;

internal class ClientHostedService : IHostedService
{
    private readonly ILogger<ClientHostedService> _logger;
    private readonly HubConnection _connection;
    private readonly RhoAiasClientOptions _options;
    private readonly Client _client;

    public ClientHostedService(ILogger<ClientHostedService> logger, IOptions<RhoAiasClientOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _client = new Client
        {
            Token = _options.Token,
            Proxies = _options.Proxies
        };
        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri($"{_options.Url}/clienthub"), config =>
            {
                config.SkipNegotiation = true;
                config.Transports = HttpTransportType.WebSockets;
                config.AccessTokenProvider = () => Task.FromResult(_options.Token);
            })
            .WithAutomaticReconnect()
            .AddMessagePackProtocol()
            .Build();
        // note: Do not use the async method
        _connection.On<string, Proxy>("CreateForwarder", CreateForwarder);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _connection.StartAsync(cancellationToken);
        await RegisterAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _connection.StopAsync(cancellationToken);
    }

    private async Task RegisterAsync()
    {
        await _connection.InvokeAsync("Register", _client);
    }

    private void CreateForwarder(string requestId, Proxy proxy)
    {
        Task.Run(async () =>
        {
            _logger.LogInformation($"CreateForwarder: {proxy.LocalIP}:{proxy.LocalPort}");
            var cancellationToken = CancellationToken.None;
            using (var serverStream = await CreateRemote(requestId, cancellationToken))
            using (var localStream = await CreateLocal(requestId, proxy.LocalIP, proxy.LocalPort, cancellationToken))
            {
                var taskX = serverStream.CopyToAsync(localStream, cancellationToken);
                var taskY = localStream.CopyToAsync(serverStream, cancellationToken);
                await Task.WhenAny(taskX, taskY);
            }
        });
    }

    private async Task<Stream> CreateLocal(string requestId, string localIp, int port, CancellationToken cancellationToken)
    {
        var socket = await ConnectAsync(localIp, port);
        return new NetworkStream(socket, true) { ReadTimeout = 1000 * 60 * 10 };
    }

    private async Task<Stream> CreateRemote(string requestId, CancellationToken cancellationToken)
    {
        var uri = new Uri($"{_options.Url}");
        var socket = await ConnectAsync(uri.Host, uri.Port);
        var serverStream = new NetworkStream(socket, true) { ReadTimeout = 1000 * 60 * 10 };
        var reverse = $"PROXY /{requestId} HTTP/1.1\r\nHost: {uri.Host}:{uri.Port}\r\n\r\n";
        var requestMsg = Encoding.UTF8.GetBytes(reverse);
        await serverStream.WriteAsync(requestMsg, cancellationToken);
        return serverStream;
    }

    private async Task<Socket> ConnectAsync(string host, int port)
    {
        _logger.LogInformation($"Create socket: {host}:{port}");
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var dnsEndPoint = new DnsEndPoint(host, port);
        await socket.ConnectAsync(dnsEndPoint);
        return socket;
    }
}
