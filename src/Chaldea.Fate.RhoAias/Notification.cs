using Microsoft.AspNetCore.SignalR;

namespace Chaldea.Fate.RhoAias;

public interface INotificationManager
{
	void StartNotification();
	Task StopNotificationAsync();
}

internal class NotificationManager : INotificationManager
{
	private readonly IHubContext<UserHub> _userHubContext;
	private readonly IMetrics _metrics;
	private Timer _metricsTimer;

	public NotificationManager(IHubContext<UserHub> userHubContext, IMetrics metrics)
	{
		_userHubContext = userHubContext;
		_metrics = metrics;
	}

	public void StartNotification()
	{
		EnableMetricNotification();
	}

	public async Task StopNotificationAsync()
	{
		await _metricsTimer.DisposeAsync();
	}

	private void EnableMetricNotification()
	{
		_metricsTimer = new Timer(state =>
		{
			var metrics = _metrics.GetMetrics();
			_userHubContext.Clients.All.SendCoreAsync("metrics", new[] { metrics });
		}, null, 3000, 3000);
	}
}