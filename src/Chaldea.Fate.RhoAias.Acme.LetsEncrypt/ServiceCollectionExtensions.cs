using Chaldea.Fate.RhoAias;
using Chaldea.Fate.RhoAias.Acme.LetsEncrypt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IRhoAiasConfigurationBuilder AddRhoAiasLetsEncrypt(this IRhoAiasConfigurationBuilder builder)
    {
        builder.Services.AddRhoAiasLetsEncrypt();
        return builder;
    }

    public static IRhoAiasApplicationBuilder UseRhoAiasLetsEncrypt(this IRhoAiasApplicationBuilder app)
    {
        app.EndpointRouteBuilder.MapRhoAiasLetsEncrypt();
        return app;
    }

    public static IServiceCollection AddRhoAiasLetsEncrypt(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddOptions<RhoAiasLetsEncryptOptions>("RhoAias:Acme:LetsEncrypt");
        services.AddKeyedSingleton<IAcmeProvider, LetsEncryptAcmeProvider>("LetsEncrypt");
        return services;
    }

    public static IEndpointRouteBuilder MapRhoAiasLetsEncrypt(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/.well-known/acme-challenge/{token}",
            async (IMemoryCache cache, HttpContext context, string token) =>
            {
                if (cache.TryGetValue(token, out var value))
                {
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync(value.ToString());
                    return;
                }

                context.Response.StatusCode = 404;
            }).ExcludeFromDescription(); // ignore for swagger
        return routeBuilder;
    }
}