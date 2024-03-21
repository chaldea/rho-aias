using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Chaldea.Fate.RhoAias;

internal class ForwarderMiddleware
{
    private readonly IForwarderManager _forwarderManager;
    private readonly RequestDelegate _next;
    private readonly RhoAiasServerOptions _options;

    public ForwarderMiddleware(
        RequestDelegate next,
        IOptions<RhoAiasServerOptions> options,
        IForwarderManager forwarderManager)
    {
        _next = next;
        _forwarderManager = forwarderManager;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Connection.LocalPort != _options.Bridge)
        {
            await _next(context);
            return;
        }

        if (context.Request.Method != "PROXY")
        {
            await _next(context);
            return;
        }

        var lifetime = context.Features.Get<IConnectionLifetimeFeature>();
        var transport = context.Features.Get<IConnectionTransportFeature>();
        if (lifetime == null || transport == null) return;

        var requestId = context.Request.Path.Value.Trim('/');
        await _forwarderManager.ForwardAsync(requestId, lifetime, transport);
    }
}