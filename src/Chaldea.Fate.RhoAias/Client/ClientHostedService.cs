using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chaldea.Fate.RhoAias;

internal class ClientHostedService : IHostedService
{
    private readonly IClientConnection _clientConnection;


    public ClientHostedService(IClientConnection clientConnection)
    {
        _clientConnection = clientConnection;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _clientConnection.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _clientConnection.StopAsync(cancellationToken);
    }
}

public class SignalRRetryPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return TimeSpan.FromSeconds(5);
    }
}

public interface IClientConnection
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task InvokeAsync<T>(string name, object[] args, CancellationToken cancellationToken);
}

internal class ClientConnection : IClientConnection
{
    private readonly Client _client;
    private readonly IConfiguration _configuration;
    private readonly ICompressor _compressor;
    private readonly HubConnection _connection;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClientHostedService> _logger;
    private readonly RhoAiasClientOptions _options;
    private Token? _token;

    public ClientConnection(
        ILogger<ClientHostedService> logger,
        IOptions<RhoAiasClientOptions> options,
        IConfiguration configuration,
        ICompressor compressor)
    {
        var version = Utilities.GetVersionName();
        logger.LogInformation($"RhoAias Client Version: {version}");
        logger.LogInformation($"RhoAias Client Key: {options.Value.Token}");
        _logger = logger;
        _configuration = configuration;
        _compressor = compressor;
        _options = options.Value;
        _client = new Client
        {
            Version = version
        };
        _httpClient = new HttpClient();
        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri($"{_options.ServerUrl}/clienthub"), config =>
            {
                config.SkipNegotiation = true;
                config.Transports = HttpTransportType.WebSockets;
                config.AccessTokenProvider = GetTokenAsync;
            })
            .AddJsonProtocol(x => { x.PayloadSerializerOptions.TypeInfoResolver = SourceGenerationContext.Default; })
            .WithAutomaticReconnect(new SignalRRetryPolicy())
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to the server.");
            Environment.Exit(0);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _connection.StopAsync(cancellationToken);
    }

    public Task InvokeAsync<T>(string name, object[] args, CancellationToken cancellationToken)
    {
        return _connection.InvokeCoreAsync<T>(name, args, cancellationToken);
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

    private Task Connection_Closed(Exception? ex)
    {
        _logger.LogError("Connection closed");
        if (ex != null && ex.GetBaseException().Message.Contains("401"))
        {
            // 401 error clean local token
            _token = null;
        }

        return Task.CompletedTask;
    }

    private async Task RegisterAsync(CancellationToken cancellationToken)
    {
        // register client info
        var result = await _connection.InvokeAsync<Result>("Register", _client, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogError(result.Message);
            await _connection.StopAsync(cancellationToken);
            return;
        }

        // todo: IngressController not belong here. need to refactor.
        // register proxies
        if (!_configuration.GetValue<bool>("RhoAias:IngressController:Enable"))
        {
            await _connection.InvokeAsync("UpdateProxy", _options.Proxies, cancellationToken);
        }
    }

    private void CreateForwarder(string requestId, Proxy proxy)
    {
        Task.Run(async () =>
        {
            _logger.LogInformation($"CreateForwarder: {proxy.LocalIP}:{proxy.LocalPort}");
            var cancellationToken = CancellationToken.None;
            using (var serverStream = await CreateRemote(requestId, cancellationToken))
            using (var localStream = await CreateLocal(proxy.Type, proxy.LocalIP, proxy.LocalPort, cancellationToken))
            {
                if (proxy.Compressed)
                {
                    using (var uncompressed = _compressor.Decompress(serverStream))
                    using (var compressed = _compressor.Compress(serverStream))
                    {
                        var taskX = uncompressed.CopyToAsync(localStream, cancellationToken);
                        var taskY = localStream.CopyToAsync(compressed, true, cancellationToken);
                        await Task.WhenAny(taskX, taskY);
                    }
                }
                else
                {
                    var taskX = serverStream.CopyToAsync(localStream, cancellationToken);
                    var taskY = localStream.CopyToAsync(serverStream, cancellationToken);
                    await Task.WhenAny(taskX, taskY);
                }
            }
        });
    }

    private async Task<Stream> CreateLocal(ProxyType type, string localIp, int port, CancellationToken cancellationToken)
    {
        if (type == ProxyType.UDP)
        {
            _logger.LogInformation($"Create socket: {localIp}:{port}");
            var client = new UdpClient();
            client.Connect(localIp, port);
            return client.GetStream();
        }
        else
        {
            var socket = await ConnectAsync(localIp, port);
            return new NetworkStream(socket, true) { ReadTimeout = 1000 * 60 * 10 };
        }
    }

    private async Task<Stream> CreateRemote(string requestId, CancellationToken cancellationToken)
    {
        var uri = new Uri($"{_options.ServerUrl}");
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

    private async Task<string?> GetTokenAsync()
    {
        if (_token != null)
        {
            return _token.AccessToken;
        }

        var response = await _httpClient.SendAsync(new HttpRequestMessage
        {
            Method = new HttpMethod("TOKEN"),
            RequestUri = new Uri($"{_options.ServerUrl}/?token_key={_options.Token}", UriKind.RelativeOrAbsolute)
        });

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            _token = JsonSerializer.Deserialize(json, SourceGenerationContext.Default.Token);
            if (_token != null)
            {
                _logger.LogInformation($"RhoAias Client Token: {_token.AccessToken}");
                return _token.AccessToken;
            }
        }
        else
        {
            var resultJson = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(resultJson))
            {
                var result = JsonSerializer.Deserialize(resultJson, SourceGenerationContext.Default.Result);
                _logger.LogError(result.Message);
            }
        }

        return null;
    }
}
