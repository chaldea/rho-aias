using Chaldea.Fate.RhoAias;

namespace Custom.Server;

public class CustomService : IHostedService
{
    private readonly IClientManager _clientManager;
    private readonly IProxyManager _proxyManager;

    public CustomService(IProxyManager proxyManager, IClientManager clientManager)
    {
        _proxyManager = proxyManager;
        _clientManager = clientManager;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task InitAsync()
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Testing",
            Token = "1234567890"
        };

        await _clientManager.CreateClientAsync(client);

        var proxy = new Proxy
        {
            Id = Guid.NewGuid(),
            Name = "ForwardToClient",
            ClientId = client.Id,
            Type = ProxyType.HTTP,
            Hosts = new[] { "localhost:5008" },
            Path = "/client/{**catch-all}", // only forward specify path
            Destination = "http://localhost:5283"
        };
        proxy.EnsureLocalIp();
        await _proxyManager.CreateProxyAsync(proxy);
    }
}