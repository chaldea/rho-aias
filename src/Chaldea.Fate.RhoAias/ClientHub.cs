using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias;

[Authorize(Roles = Role.Client)]
internal class ClientHub : Hub
{
    private readonly ILogger<ClientHub> _logger;
    private readonly IClientManager _clientManager;

    public ClientHub(
	    ILogger<ClientHub> logger,
	    IClientManager clientManager)
    {
        _logger = logger;
        _clientManager = clientManager;
    }

    [HubMethodName("Register")]
    public async Task<Result> RegisterAsync(Client register)
    {
	    register.Update(Context);
	    return await _clientManager.RegisterClientAsync(register);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        await _clientManager.UnRegisterClientAsync(Context.ConnectionId);
	}
}
