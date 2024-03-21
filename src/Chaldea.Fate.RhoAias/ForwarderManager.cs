using System.Collections.Concurrent;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Chaldea.Fate.RhoAias;

internal interface IForwarderManager
{
    void Register(Proxy[] proxies);
    void UnRegister(Proxy[] proxies);
    Task ForwardAsync(string requestId, IConnectionLifetimeFeature lifetime, IConnectionTransportFeature transport);
    ValueTask<Stream> CreateAndWaitAsync(Uri? uri, CancellationToken cancellation);
}

internal class ForwarderManager : IForwarderManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Guid, IForwarder> _forwarders = new();

    public ForwarderManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Register(Proxy[] proxies)
    {
        foreach (var proxy in proxies)
        {
            var forwarder = _serviceProvider.GetKeyedService<IForwarder>(proxy.Type);
            if (forwarder == null) continue;
            forwarder.Register(proxy);
            _forwarders.TryAdd(proxy.Id, forwarder);
        }
    }

    public void UnRegister(Proxy[] proxies)
    {
        foreach (var proxy in proxies)
        {
            if (_forwarders.TryRemove(proxy.Id, out var forwarder))
            {
                forwarder.UnRegister();
            }
        }
    }

    public async ValueTask<Stream> CreateAndWaitAsync(Uri? uri, CancellationToken cancellation)
    {
        // There may be more than one, just take one of them.
        var forwarder = _forwarders.Values.FirstOrDefault(x => x.Proxy.HasUri(uri));
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