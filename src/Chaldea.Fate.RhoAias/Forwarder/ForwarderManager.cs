using System.Collections.Concurrent;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Chaldea.Fate.RhoAias;

internal interface IForwarderManager
{
    void Register(Proxy? proxy);
    void Register(List<Proxy> proxies);
    void UnRegister(List<Proxy> proxies);
    void UnRegister(Proxy? proxy);
    void UnRegister(Guid id);
    Task ForwardAsync(string requestId, IConnectionLifetimeFeature lifetime, IConnectionTransportFeature transport);
    ValueTask<Stream> CreateAndWaitAsync(string proxyName, CancellationToken cancellation);
}

internal class ForwarderManager : IForwarderManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Guid, IForwarder> _forwarders = new();

    public ForwarderManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Register(List<Proxy> proxies)
    {
        foreach (var proxy in proxies)
        {
            Register(proxy);
        }
    }

    public void Register(Proxy? proxy)
    {
        if (proxy == null) return;
        if (proxy.Disabled) return;
        if (proxy.Client is not { Status: true }) return;
        var forwarder = _serviceProvider.GetKeyedService<IForwarder>(proxy.Type);
        if (forwarder == null) return;
        if (_forwarders.TryAdd(proxy.Id, forwarder))
        {
            forwarder.Register(proxy);
        }
    }

    public void UnRegister(List<Proxy> proxies)
    {
        foreach (var proxy in proxies)
        {
            UnRegister(proxy);
        }
    }

    public void UnRegister(Proxy? proxy)
    {
        if (proxy == null) return;
        UnRegister(proxy.Id);
    }

    public void UnRegister(Guid id)
    {
        if (_forwarders.TryRemove(id, out var forwarder))
        {
            forwarder.UnRegister();
        }
    }

    public async ValueTask<Stream> CreateAndWaitAsync(string proxyName, CancellationToken cancellation)
    {
        var forwarder = _forwarders.Values.FirstOrDefault(x => x.Proxy.Name == proxyName);
        if (forwarder != null)
        {
            return await forwarder.CreateAsync(cancellation);
        }

        return Stream.Null;
    }

    public async Task ForwardAsync(string requestId, IConnectionLifetimeFeature lifetime, IConnectionTransportFeature transport)
    {
        foreach (var forwarder in _forwarders.Values)
        {
            await forwarder.ForwardAsync(requestId, lifetime, transport);
        }
    }
}