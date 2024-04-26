using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chaldea.Fate.RhoAias;

internal class ServerHostedService : IHostedService
{
	private readonly ILogger<ServerHostedService> _logger;
	private readonly IOptions<RhoAiasServerOptions> _options;
	private readonly IClientManager _clientManager;
	private readonly INotificationManager _notificationManager;

	public ServerHostedService(
		ILogger<ServerHostedService> logger,
		IOptions<RhoAiasServerOptions> options, 
		IClientManager clientManager, 
		INotificationManager notificationManager)
	{
		_logger = logger;
		_options = options;
		_clientManager = clientManager;
		_notificationManager = notificationManager;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Initialize metrics collector.");
		await _clientManager.InitClientMetricsAsync();
		_logger.LogInformation("Initialize client data.");
		await _clientManager.ResetClientStatusAsync();
		if (_options.Value.EnableNotification)
		{
			_logger.LogInformation("Start notification.");
			_notificationManager.StartNotification();
		}
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		if (_options.Value.EnableNotification)
		{
			_logger.LogInformation("Stop notification.");
			await _notificationManager.StopNotificationAsync();
		}
	}
}