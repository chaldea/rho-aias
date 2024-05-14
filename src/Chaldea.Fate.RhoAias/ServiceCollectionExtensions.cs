using Chaldea.Fate.RhoAias;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using Microsoft.Extensions.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private const string GlobalEndpointRouteBuilder = "__GlobalEndpointRouteBuilder";

    public static IServiceCollection AddRhoAias(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RhoAiasServerOptions>(configuration.GetSection("RhoAias:Server"));
        services.AddReverseProxy().LoadFromMemory();
        services.AddHttpContextAccessor();
        services.AddSignalR().AddMessagePackProtocol();
        services.AddSingleton<IForwarderHttpClientFactory, WebForwarderHttpClientFactory>();
		services.AddSingleton<IForwarderManager, ForwarderManager>();
		services.AddSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));
		services.AddKeyedTransient<IForwarder, WebForwarder>(ProxyType.HTTP);
        services.AddKeyedTransient<IForwarder, WebForwarder>(ProxyType.HTTPS);
        services.AddKeyedTransient<IForwarder, PortForwarder>(ProxyType.TCP);
        services.AddKeyedTransient<IForwarder, PortForwarder>(ProxyType.UDP);
        services.AddSingleton<ITokenManager, TokenManager>();
		services.AddSingleton<IClientManager, ClientManager>();
		services.AddSingleton<IProxyManager, ProxyManager>();
		services.AddSingleton<ICertManager, CertManager>();
		services.AddSingleton<IUserManager, UserManager>();
		services.AddSingleton<IDnsProviderManager, DnsProviderManager>();
		services.AddSingleton<IMetrics, Metrics>();
		services.AddSingleton<IServerCertificateSelector, DefaultServerCertificateSelector>();
		services.AddHostedService<ServerHostedService>();
		services.AddHostedService<CertRenewJob>();
		return services;
    }

	public static IApplicationBuilder UseRhoAias(this IApplicationBuilder app)
    {
        app.UseWebSockets();
        app.UseMiddleware<ForwarderMiddleware>();
        if (app.Properties.TryGetValue(GlobalEndpointRouteBuilder, out var obj))
        {
            var endpoint = obj as IEndpointRouteBuilder;
            endpoint?.MapReverseProxy();
            endpoint?.MapHub<ClientHub>("/clienthub");
			endpoint?.MapHub<UserHub>("/userhub");
        }
        return app;
    }

	public static IWebHostBuilder ConfigureRhoAiasServer(this IWebHostBuilder builder)
	{
        builder.ConfigureKestrel((context, serverOptions) =>
        {
	        var options = new RhoAiasServerOptions();
	        context.Configuration.GetSection("RhoAias:Server").Bind(options);
	        var certificateSelector = serverOptions.ApplicationServices.GetService<IServerCertificateSelector>();
			serverOptions.Listen(IPAddress.Any, options.Bridge);
	        serverOptions.Listen(IPAddress.Any, options.Http);
	        serverOptions.Listen(IPAddress.Any, options.Https, listenOptions =>
	        {
		        listenOptions.UseHttps(httpsOptions =>
		        {
			        if (certificateSelector is null)
			        {
				        throw new InvalidOperationException("");
			        }
			        var fallbackSelector = httpsOptions.ServerCertificateSelector;
			        httpsOptions.ServerCertificateSelector = (connectionContext, domainName) =>
			        {
				        var primaryCert = certificateSelector.Select(connectionContext!, domainName);
				        return primaryCert ?? fallbackSelector?.Invoke(connectionContext, domainName);
			        };
				});
	        });
        });
		return builder;
	}

	public static WebApplicationBuilder AddRhoAiasServer(this WebApplicationBuilder builder, Action<IRhoAiasConfigurationBuilder>? config = default)
	{
		builder.Services.AddRhoAias(builder.Configuration);
		builder.WebHost.ConfigureRhoAiasServer();
		var configBuilder = new RhoAiasConfigurationBuilder(builder.Services, builder.Configuration);
		config?.Invoke(configBuilder);
		return builder;
	}

    public static WebApplication UseRhoAiasServer(this WebApplication app, Action<IRhoAiasApplicationBuilder>? builder = default)
    {
        app.UseRhoAias();
        var appBuilder = new RhoAiasApplicationBuilder(app);
        builder?.Invoke(appBuilder);
		return app;
    }

    public static IServiceCollection AddRhoAiasClient(this IServiceCollection services, IConfiguration configuration)
    {
	    services.Configure<RhoAiasClientOptions>(configuration.GetSection("RhoAias:Client"));
		services.AddSingleton<IClientConnection, ClientConnection>();
		services.AddHostedService<ClientHostedService>();
	    return services;
    }
}