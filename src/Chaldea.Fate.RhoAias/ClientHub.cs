using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias;

public class ClientHub : Hub
{
    private readonly ILogger<ClientHub> _logger;
    private readonly IClientManager _clientManager;

    public ClientHub(ILogger<ClientHub> logger, IClientManager clientManager)
    {
        _logger = logger;
        _clientManager = clientManager;
    }

    [HubMethodName("Register")]
    public Task RegisterAsync(Client client)
    {
        client.ConnectionId = Context.ConnectionId;
        _clientManager.Register(client);
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _clientManager.UnRegister(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
