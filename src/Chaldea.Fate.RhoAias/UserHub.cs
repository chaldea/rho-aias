using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias;

[Authorize(Roles = Role.User)]
internal class UserHub : Hub
{
	private readonly ILogger<UserHub> _logger;

	public UserHub(ILogger<UserHub> logger)
	{
		_logger = logger;
	}

	public override Task OnConnectedAsync()
	{
		var userId = Context.User.UserId();
		_logger.LogInformation($"User connected: {userId}");
		return base.OnConnectedAsync();
	}
}