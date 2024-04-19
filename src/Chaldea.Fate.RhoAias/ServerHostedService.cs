using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Chaldea.Fate.RhoAias;

internal class ServerHostedService : IHostedService
{
	private readonly IOptions<RhoAiasServerOptions> _options;
	private readonly IClientManager _clientManager;
	private readonly INotificationManager _notificationManager;

	public ServerHostedService(
		IOptions<RhoAiasServerOptions> options, 
		IClientManager clientManager, 
		INotificationManager notificationManager)
	{
		_options = options;
		_clientManager = clientManager;
		_notificationManager = notificationManager;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await _clientManager.InitClientMetricsAsync();
		await _clientManager.ResetClientStatusAsync();
		if (_options.Value.EnableNotification)
		{
			_notificationManager.EnableNotification();
		}
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}