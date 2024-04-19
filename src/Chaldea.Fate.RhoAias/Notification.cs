using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias;

[Authorize(Roles = Role.User)]
internal class NotificationHub : Hub
{
	private readonly ILogger<NotificationHub> _logger;

	public NotificationHub(ILogger<NotificationHub> logger)
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

public interface INotificationManager
{
	void EnableNotification();
}

internal class NotificationManager : INotificationManager
{
	private readonly IHubContext<NotificationHub> _hubContext;
	private readonly IMetrics _metrics;
	private Timer _metricsTimer;

	public NotificationManager(IHubContext<NotificationHub> hubContext, IMetrics metrics)
	{
		_hubContext = hubContext;
		_metrics = metrics;
	}

	public void EnableNotification()
	{
		EnableMetricNotification();
	}

	private void EnableMetricNotification()
	{
		_metricsTimer = new Timer(state =>
		{
			var metrics = _metrics.GetMetrics();
			_hubContext.Clients.All.SendCoreAsync("metrics", new[] { metrics });
		}, null, 3000, 3000);
	}
}