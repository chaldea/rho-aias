using Yarp.Kubernetes.Controller.Client;

namespace Chaldea.Fate.RhoAias.IngressController;

internal class IngressResourceStatusUpdater : IIngressResourceStatusUpdater
{
	public Task UpdateStatusAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}