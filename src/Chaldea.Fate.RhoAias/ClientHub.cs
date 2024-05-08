using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias;

[Authorize(Roles = Role.Client)]
internal class ClientHub : Hub
{
    private readonly ILogger<ClientHub> _logger;
    private readonly IClientManager _clientManager;
    private readonly IProxyManager _proxyManager;

    public ClientHub(
	    ILogger<ClientHub> logger,
	    IClientManager clientManager,
	    IProxyManager proxyManager)
    {
        _logger = logger;
        _clientManager = clientManager;
        _proxyManager = proxyManager;
    }

    public override Task OnConnectedAsync()
    {
	    _logger.LogInformation($"Client connected: {Context.ConnectionId}");
	    return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
	    _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
		await base.OnDisconnectedAsync(exception);
	    await _clientManager.UnRegisterClientAsync(Context.ConnectionId);
    }

	[HubMethodName("Register")]
    public async Task<Result> RegisterAsync(Client register)
    {
	    register.Update(Context);
	    return await _clientManager.RegisterClientAsync(register);
    }

    [HubMethodName("UpdateProxy")]
	public async Task UpdateProxyAsync(List<Proxy> proxies)
	{
		var clientId = Context.User?.UserId() ?? Guid.Empty;
		await _proxyManager.UpdateProxyListAsync(clientId, proxies);
	}
}