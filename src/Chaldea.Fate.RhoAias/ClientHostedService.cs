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
	    var version = Utilities.GetVersionName();
		logger.LogInformation($"RhoAias Client Version: {version}");
		_logger = logger;
        _options = options.Value;
        _client = new Client
        {
            Version = version,
			Proxies = _options.Proxies
        };
        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri($"{_options.Url}/clienthub"), config =>
            {
                config.SkipNegotiation = true;
                config.Transports = HttpTransportType.WebSockets;
                config.AccessTokenProvider = () => Task.FromResult(_options.Token);
            })
            .WithAutomaticReconnect(new SignalRRetryPolicy())
            .AddMessagePackProtocol()
            .Build();
		_connection.Reconnecting += Connection_Reconnecting;
		_connection.Reconnected += Connection_Reconnected;
		_connection.Closed += Connection_Closed;
		// note: Do not use the async method
		_connection.On<string, Proxy>("CreateForwarder", CreateForwarder);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
	    try
	    {
		    await _connection.StartAsync(cancellationToken);
		    await RegisterAsync(cancellationToken);
	    }
	    catch
	    {
		    _logger.LogError("Failed to connect to the server.");
		    Environment.Exit(0);
		}
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
	    await _connection.StopAsync(cancellationToken);
    }

	private Task Connection_Reconnecting(Exception? arg)
    {
	    _logger.LogInformation("Connection reconnecting...");
        return Task.CompletedTask;
    }

	private async Task Connection_Reconnected(string? arg)
	{
		_logger.LogInformation("Connection reconnected.");
		await RegisterAsync(CancellationToken.None);
	}

	private Task Connection_Closed(Exception? arg)
	{
		_logger.LogError("Connection closed");
        return Task.CompletedTask;
	}

	private async Task RegisterAsync(CancellationToken cancellationToken)
	{
		var result = await _connection.InvokeAsync<Result>("Register", _client, cancellationToken);
		if (!result.IsSuccess)
		{
            _logger.LogError(result.Message);
            await _connection.StopAsync(cancellationToken);
		}
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

public class SignalRRetryPolicy : IRetryPolicy
{
	public TimeSpan? NextRetryDelay(RetryContext retryContext)
	{
		return TimeSpan.FromSeconds(5);
	}
}