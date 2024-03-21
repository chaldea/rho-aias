using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias;

public interface IClientManager
{
    void Register(Client client);
    void UnRegister(string connectionId);
}

internal class ClientManager : IClientManager
{
    private readonly ILogger<ClientManager> _logger;
    private readonly IForwarderManager _forwarderManager;
    private readonly List<Client> _clients = new();

    public ClientManager(ILogger<ClientManager> logger, IForwarderManager forwarderManager)
    {
        _logger = logger;
        _forwarderManager = forwarderManager;
    }

    public void Register(Client client)
    {
        var entity = _clients.FirstOrDefault(x => x.Token == client.Token);
        if (entity == null)
        {
            entity = client;
            entity.Id = Guid.NewGuid();
            _clients.Add(entity);
        }
        if (entity.Proxies is { Length: > 0 })
        {
            foreach (var proxy in entity.Proxies)
            {
                proxy.Id = Guid.NewGuid();
                proxy.Client = client;
            }
            _forwarderManager.Register(client.Proxies);
        }
    }

    public void UnRegister(string connectionId)
    {
        var client = _clients.FirstOrDefault(x => x.ConnectionId == connectionId);
        if (client != null)
        {
            _forwarderManager.UnRegister(client.Proxies);
            _clients.Remove(client);
        }
    }
}