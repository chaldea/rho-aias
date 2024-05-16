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
		var options = new RhoAiasIngressControllerOptions();
		configuration.GetSection("RhoAias:IngressController").Bind(options);
		services.AddKubernetesControllerRuntime(configuration);
		services.Replace(new ServiceDescriptor(typeof(IIngressResourceStatusUpdater), typeof(IngressResourceStatusUpdater), ServiceLifetime.Singleton));
		services.AddSingleton<IUpdateConfig, KubernetesConfigProvider>();
		services.Configure<YarpOptions>(yarp =>
		{
			// reset YarpOptions
			yarp.ControllerClass = options.ControllerClass;
			yarp.ServerCertificates = options.ServerCertificates;
			yarp.DefaultSslCertificate = options.DefaultSslCertificate;
			yarp.ControllerServiceName = options.ControllerServiceName;
			yarp.ControllerServiceNamespace = options.ControllerServiceNamespace;
		});
		return services;
	}
}