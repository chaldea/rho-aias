using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Chaldea.Fate.RhoAias.Dashboard;

public class SummaryDto
{
	public string Version { get; set; }
	public int BindPort { get; set; }
	public int HttpPort { get; set; }
	public int HttpsPort { get; set; }
	public int Proxies { get; set; }
	public int Certs { get; set; }
}

[Authorize(Roles = Role.User)]
[ApiController]
[Route("api/dashboard/statistic")]
public class StatisticController
{
	private readonly IMetrics _metrics;
	private readonly IProxyManager _proxyManager;
	private readonly IOptions<RhoAiasServerOptions> _serverOptions;

	public StatisticController(IMetrics metrics, IProxyManager proxyManager, IOptions<RhoAiasServerOptions> serverOptions)
	{
		_metrics = metrics;
		_proxyManager = proxyManager;
		_serverOptions = serverOptions;
	}

	[HttpGet]
	[Route("metrics")]
	public Task<object> GetMetricsAsync()
	{
		object metrics = _metrics.GetMetrics();
		return Task.FromResult(metrics);
	}

	[HttpGet]
	[Route("summary")]
	public async Task<SummaryDto> GetSummaryAsync()
	{
		var summary = new SummaryDto
		{
			Proxies = await _proxyManager.CountProxyAsync(),
			Certs = 0,
			BindPort = _serverOptions.Value.Bridge,
			HttpPort = _serverOptions.Value.Http,
			HttpsPort = _serverOptions.Value.Https,
			Version = typeof(RhoAiasServerOptions).Assembly.GetName().Version?.ToString()
		};
		return summary;
	}
}