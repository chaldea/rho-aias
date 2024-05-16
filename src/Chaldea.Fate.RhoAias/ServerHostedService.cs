using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chaldea.Fate.RhoAias;

internal class ServerHostedService : BackgroundService
{
	private readonly ILogger<ServerHostedService> _logger;
	private readonly IOptions<RhoAiasServerOptions> _options;
	private readonly IClientManager _clientManager;
	private readonly IHubContext<UserHub> _userHubContext;
	private readonly IMetrics _metrics;

	public ServerHostedService(
		ILogger<ServerHostedService> logger,
		IOptions<RhoAiasServerOptions> options, 
		IClientManager clientManager,
		IHubContext<UserHub> userHubContext,
		IMetrics metrics)
	{
		_logger = logger;
		_options = options;
		_clientManager = clientManager;
		_userHubContext = userHubContext;
		_metrics = metrics;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await InitializeAsync(stoppingToken);
		await MonitorAsync(stoppingToken);
	}

	private async Task InitializeAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Initialize metrics collector.");
		await _clientManager.InitClientMetricsAsync();
		_logger.LogInformation("Initialize client data.");
		await _clientManager.ResetClientStatusAsync();
	}

	private async Task MonitorAsync(CancellationToken stoppingToken)
	{
		if (_options.Value.EnableMetricsMonitor)
		{
			using PeriodicTimer timer = new(TimeSpan.FromSeconds(5));
			try
			{
				while (await timer.WaitForNextTickAsync(stoppingToken))
				{
					try
					{
						var metrics = _metrics.GetMetrics();
						await _userHubContext.Clients.All.SendCoreAsync("metrics", new[] { metrics });
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "");
					}
				}
			}
			catch (OperationCanceledException)
			{
				_logger.LogInformation("MetricsMonitor Service is stopping.");
			}
		}
	}
}