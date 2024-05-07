using Microsoft.Extensions.Logging;
using Yarp.Kubernetes.Controller.Caching;
using Yarp.Kubernetes.Controller.Configuration;
using Yarp.ReverseProxy.Configuration;

namespace Chaldea.Fate.RhoAias.IngressController;

internal class KubernetesConfigProvider : IUpdateConfig
{
	private readonly ILogger<KubernetesConfigProvider> _logger;

	public KubernetesConfigProvider(ILogger<KubernetesConfigProvider> logger, ICache cache)
	{
		_logger = logger;
	}

	public Task UpdateAsync(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters, CancellationToken cancellationToken)
	{
		// todo: register yarp config to server
		_logger.LogWarning("Kubernetes config updated...");
		return Task.CompletedTask;
	}
}