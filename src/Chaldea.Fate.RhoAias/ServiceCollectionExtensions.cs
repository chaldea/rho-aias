using Chaldea.Fate.RhoAias;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using Microsoft.Extensions.Configuration;
using Yarp.ReverseProxy.Forwarder;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRhoAiasServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RhoAiasServerOptions>(configuration.GetSection("RhoAias:Server"));
        services.AddReverseProxy().LoadFromMemory();
        services.AddHttpContextAccessor();
        services.AddSignalR().AddMessagePackProtocol();
        services.AddSingleton<IForwarderHttpClientFactory, CustomForwarderHttpClientFactory>();
        services.AddSingleton<IClientManager, ClientManager>();
        services.AddSingleton<IForwarderManager, ForwarderManager>();
        services.AddKeyedTransient<IForwarder, WebForwarder>(ProxyType.HTTP);
        services.AddKeyedTransient<IForwarder, WebForwarder>(ProxyType.HTTPS);
        services.AddKeyedTransient<IForwarder, PortForwarder>(ProxyType.TCP);
        services.AddKeyedTransient<IForwarder, PortForwarder>(ProxyType.UDP);
        return services;
    }

    public static IWebHostBuilder ConfigureRhoAiasServer(this IWebHostBuilder builder)
    {
        builder.ConfigureKestrel((context, serverOptions) =>
        {
            var options = new RhoAiasServerOptions();
            context.Configuration.GetSection("RhoAias:Server").Bind(options);
            serverOptions.Listen(IPAddress.Any, options.Bridge);
            serverOptions.Listen(IPAddress.Any, options.Http);
            serverOptions.Listen(IPAddress.Any, options.Https, listenOptions =>
            {
                listenOptions.UseHttps();
            });
        });
        return builder;
    }

    public static WebApplicationBuilder AddRhoAiasServer(this WebApplicationBuilder builder)
    {
        builder.Services.AddRhoAiasServer(builder.Configuration);
        builder.WebHost.ConfigureRhoAiasServer();
        return builder;
    }

    public static WebApplication UseRhoAiasServer(this WebApplication app)
    {
        app.UseWebSockets();
        app.UseMiddleware<ForwarderMiddleware>();
        app.MapReverseProxy();
        app.MapHub<ClientHub>("/clienthub");
        return app;
    }

    public static IServiceCollection AddRhoAiasClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RhoAiasClientOptions>(configuration.GetSection("RhoAias:Client"));
        services.AddHostedService<ClientHostedService>();
        return services;
    }
}