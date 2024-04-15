using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias;

[Authorize]
internal class ClientHub : Hub
{
    private readonly ILogger<ClientHub> _logger;
    private readonly IServerManager _serverManager;

    public ClientHub(
	    ILogger<ClientHub> logger, 
	    IServerManager serverManager)
    {
        _logger = logger;
        _serverManager = serverManager;
    }

    [HubMethodName("Register")]
    public async Task RegisterAsync(Client register)
    {
	    register.Update(Context);
	    var result = await _serverManager.RegisterClientAsync(register);
	    if (!result)
	    {
		    _logger.LogError($"Unauthorized connection：{register.Id}");
            Context.Abort();
	    }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        await _serverManager.UnRegisterClientAsync(Context.ConnectionId);
	}
}
