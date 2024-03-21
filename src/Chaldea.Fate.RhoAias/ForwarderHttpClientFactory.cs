using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;

namespace Chaldea.Fate.RhoAias;

public static class ReverseProxyBuilderExtensions
{
    public static IReverseProxyBuilder LoadFromMemory(this IReverseProxyBuilder builder)
    {
        builder.Services.AddSingleton<IProxyConfigProvider>(new InMemoryConfigProvider(Array.Empty<RouteConfig>(), Array.Empty<ClusterConfig>()));
        return builder;
    }
}

internal class CustomForwarderHttpClientFactory : ForwarderHttpClientFactory
{
    private readonly ILogger<CustomForwarderHttpClientFactory> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IForwarderManager _forwarderManager;

    public CustomForwarderHttpClientFactory(
        ILogger<CustomForwarderHttpClientFactory> logger,
        IHttpContextAccessor httpContextAccessor,
        IForwarderManager forwarderManager)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _forwarderManager = forwarderManager;
    }

    protected override void ConfigureHandler(ForwarderHttpClientContext context, SocketsHttpHandler handler)
    {
        base.ConfigureHandler(context, handler);
        handler.ConnectCallback = ConnectCallback;
    }

    public async ValueTask<Stream> ConnectCallback(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        return await _forwarderManager.CreateAndWaitAsync(context.InitialRequestMessage.RequestUri, cancellationToken);
    }
}
