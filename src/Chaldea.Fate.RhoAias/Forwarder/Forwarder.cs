using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Chaldea.Fate.RhoAias;

internal interface IForwarder
{
    Proxy Proxy { get; }
    void Register(Proxy proxy);
    void UnRegister();
    ValueTask<Stream> CreateAsync(CancellationToken cancellation);
    Task ForwardAsync(string requestId, IConnectionLifetimeFeature lifetime, IConnectionTransportFeature transport);
}

abstract class ForwarderBase : IForwarder
{
    protected Proxy _proxy;
    protected readonly ConcurrentDictionary<string, (TaskCompletionSource<Stream>, CancellationToken)> ForwarderTasks = new();
    private readonly ILogger<ForwarderBase> _logger;
    private readonly ICompressor _compressor;

    public Proxy Proxy => _proxy;

    protected ForwarderBase(IServiceProvider service)
    {
        _logger = service.GetRequiredService<ILogger<ForwarderBase>>();
        _compressor = service.GetRequiredService<ICompressor>();
    }

    public virtual void Register(Proxy proxy)
    {
        _proxy = proxy;
    }

    public abstract void UnRegister();

    public virtual ValueTask<Stream> CreateAsync(CancellationToken cancellation)
    {
        return new ValueTask<Stream>(Stream.Null);
    }

    public virtual async Task ForwardAsync(string requestId, IConnectionLifetimeFeature lifetime, IConnectionTransportFeature transport)
    {
        if (!ForwarderTasks.TryRemove(requestId, out var responseAwaiter))
        {
            return;
        }

        try
        {
            _logger.LogInformation($"Forward Starting: {requestId}");
            var compressor = _proxy.Compressed ? _compressor : null;
            using var reverseConnection = new WebSocketStream(lifetime, transport, compressor);
            responseAwaiter.Item1.TrySetResult(reverseConnection);
            CancellationTokenSource cts;
            if (responseAwaiter.Item2 != CancellationToken.None)
            {
                cts = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ConnectionClosed,
                    responseAwaiter.Item2);
            }
            else
            {
                cts = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ConnectionClosed);
            }

            var closedAwaiter = new TaskCompletionSource<object>();
            await closedAwaiter.Task.WaitAsync(cts.Token);
            _logger.LogInformation($"Forward Closed: {requestId}");
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "");
        }
    }
}
