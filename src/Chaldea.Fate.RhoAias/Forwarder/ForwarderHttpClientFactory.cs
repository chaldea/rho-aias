using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;

namespace Chaldea.Fate.RhoAias;

public static class ReverseProxyBuilderExtensions
{
    public static IReverseProxyBuilder LoadFromMemory(this IReverseProxyBuilder builder)
    {
        builder.Services.AddSingleton<IProxyConfigProvider>(new InMemoryConfigProvider(Array.Empty<RouteConfig>(),
            Array.Empty<ClusterConfig>()));
        return builder;
    }
}

internal class WebForwarderHttpClientFactory : ForwarderHttpClientFactory
{
    private readonly IForwarderManager _forwarderManager;

    public WebForwarderHttpClientFactory(IForwarderManager forwarderManager)
    {
        _forwarderManager = forwarderManager;
    }

    protected override void ConfigureHandler(ForwarderHttpClientContext context, SocketsHttpHandler handler)
    {
        base.ConfigureHandler(context, handler);
        handler.ConnectCallback = async (ctx, cancellationToken) =>
            await _forwarderManager.CreateAndWaitAsync(context.ClusterId, cancellationToken);
    }
}