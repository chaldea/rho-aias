using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;

namespace Chaldea.Fate.RhoAias;

internal class ForwarderMiddleware
{
    private readonly IForwarderManager _forwarderManager;
    private readonly RequestDelegate _next;

    public ForwarderMiddleware(
        RequestDelegate next,
        IForwarderManager forwarderManager)
    {
        _next = next;
        _forwarderManager = forwarderManager;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method != "PROXY")
        {
            await _next(context);
            return;
        }

        var lifetime = context.Features.Get<IConnectionLifetimeFeature>();
        var transport = context.Features.Get<IConnectionTransportFeature>();
        if (lifetime == null || transport == null) return;

        var requestId = context.Request.Path.Value?.Trim('/');
        if (requestId == null) return;
        await _forwarderManager.ForwardAsync(requestId, lifetime, transport);
    }
}