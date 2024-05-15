using Microsoft.Extensions.Logging;
using Yarp.Kubernetes.Controller.Configuration;
using Yarp.ReverseProxy.Configuration;

namespace Chaldea.Fate.RhoAias.IngressController;

internal class KubernetesConfigProvider : IUpdateConfig
{
	private readonly ILogger<KubernetesConfigProvider> _logger;
	private readonly IClientConnection _connection;

	public KubernetesConfigProvider(ILogger<KubernetesConfigProvider> logger, IClientConnection connection)
	{
		_logger = logger;
		_connection = connection;
	}

	public async Task UpdateAsync(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Kubernetes ingress config register.");
		var proxies = new List<Proxy>();
		foreach (var route in routes)
		{
			var cluster = clusters.FirstOrDefault(x => x.ClusterId == route.ClusterId);
			if(cluster == null) continue;
			var proxy = new Proxy(route, cluster);
			proxies.Add(proxy);
		}

		await _connection.InvokeAsync<List<Proxy>>("UpdateProxy", new[] { proxies }, cancellationToken);
	}
}