using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Configuration;

namespace Chaldea.Fate.RhoAias;

internal class HttpForwarder : ForwarderBase
{
    private readonly ILogger<ForwarderManager> _logger;
    private readonly IHubContext<ClientHub> _hub;
    private readonly IProxyConfigProvider _proxyConfigProvider;

    public HttpForwarder(
        ILogger<ForwarderManager> logger,
        IHubContext<ClientHub> hub,
        IProxyConfigProvider proxyConfigProvider,
        IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _logger = logger;
        _hub = hub;
        _proxyConfigProvider = proxyConfigProvider;
    }

    public override void Register(Proxy proxy)
    {
        base.Register(proxy);
        _logger.LogInformation($"Register http forwarder {proxy.GetHosts()} => {proxy.GetUrl()}");
        var config = _proxyConfigProvider.GetConfig();
        var routes = config.Routes.ToList();
        var clusters = config.Clusters.ToList();
        routes.RemoveAll(x => x.ClusterId == proxy.Name);
        clusters.RemoveAll(x => x.ClusterId == proxy.Name);
        if (proxy is { RouteConfig: not null, ClusterConfig: not null })
        {
            var route = JsonSerializer.Deserialize<RouteConfig>(proxy.RouteConfig);
            var cluster = JsonSerializer.Deserialize<ClusterConfig>(proxy.ClusterConfig);
            if (route != null) routes.Add(route);
            if (cluster != null) clusters.Add(cluster);
        }
        else
        {
            routes.Add(new RouteConfig
            {
                ClusterId = proxy.Name,
                RouteId = proxy.Name,
                Match = new RouteMatch { Path = proxy.Path, Hosts = proxy.Hosts }
            });
            clusters.Add(new ClusterConfig
            {
                ClusterId = proxy.Name,
                Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                {
                    { "destination", new DestinationConfig() { Address = $"{proxy.GetUrl()}" } }
                }
            });
        }

        (_proxyConfigProvider as InMemoryConfigProvider).Update(routes, clusters);
    }

    public override void UnRegister()
    {
        _logger.LogInformation($"UnRegister http forwarder {_proxy.GetHosts()} => {_proxy.GetUrl()}");
        var config = _proxyConfigProvider.GetConfig();
        var routes = config.Routes.ToList();
        var clusters = config.Clusters.ToList();
        routes.RemoveAll(x => x.RouteId == _proxy.Name);
        clusters.RemoveAll(x => x.ClusterId == _proxy.Name);
        (_proxyConfigProvider as InMemoryConfigProvider).Update(routes, clusters);
    }
}
