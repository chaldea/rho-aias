using Chaldea.Fate.RhoAias;
using Chaldea.Fate.RhoAias.IngressController;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yarp.Kubernetes.Controller;
using Yarp.Kubernetes.Controller.Client;
using Yarp.Kubernetes.Controller.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IRhoAiasConfigurationBuilder AddAhoAiasIngressController(this IRhoAiasConfigurationBuilder builder)
	{
		builder.Services.AddAhoAiasIngressController(builder.Configuration);
		return builder;
	}

	public static IServiceCollection AddAhoAiasIngressController(this IServiceCollection services, IConfiguration configuration)
	{
		if (configuration.GetValue<bool>("RhoAias:IngressController:Enable"))
		{
			services.AddKubernetesControllerRuntime(configuration);
			services.Replace(new ServiceDescriptor(typeof(IIngressResourceStatusUpdater), typeof(IngressResourceStatusUpdater), ServiceLifetime.Singleton));
			services.AddSingleton<IUpdateConfig, KubernetesConfigProvider>();
			services.Configure<YarpOptions>(configuration.GetSection("RhoAias:IngressController"));
			services.Configure<YarpOptions>(option =>
			{
				// set default value of YarpOptions
				option.ControllerClass = option.ControllerClass ?? "microsoft.com/ingress-yarp";
				option.DefaultSslCertificate = option.DefaultSslCertificate ?? "yarp/yarp-ingress-tls";
				option.ControllerServiceName = option.ControllerServiceName ?? "ingress-yarp-controller";
				option.ControllerServiceNamespace = option.ControllerServiceNamespace ?? "yarp";
			});
		}
		return services;
	}
}