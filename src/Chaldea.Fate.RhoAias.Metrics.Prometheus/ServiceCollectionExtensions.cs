using Chaldea.Fate.RhoAias;
using Chaldea.Fate.RhoAias.Metrics.Prometheus;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Metrics;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IRhoAiasConfigurationBuilder AddRhoAiasPrometheus(this IRhoAiasConfigurationBuilder builder)
    {
        builder.Services.AddRhoAiasPrometheus(builder.Configuration);
        return builder;
    }

    public static IRhoAiasApplicationBuilder UseRhoAiasPrometheus(this IRhoAiasApplicationBuilder builder)
    {
        builder.ApplicationBuilder.UseRhoAiasPrometheus();
        return builder;
    }

    public static IServiceCollection AddRhoAiasPrometheus(this IServiceCollection services, IConfiguration configuration)
    {
        var configKey = "RhoAias:Metrics:Prometheus";
        var options = new RhoAiasPrometheusOptions();
        services.AddOptions<RhoAiasPrometheusOptions>(configKey);
        configuration.GetSection(configKey).Bind(options);
        services.AddOpenTelemetry().WithMetrics(b =>
        {
            b.AddPrometheusExporter();
            b.AddMeter(options.Meters);
        });
        return services;
    }

    public static IApplicationBuilder UseRhoAiasPrometheus(this IApplicationBuilder app)
    {
        var endpoint = app.GetEndpointRoute();
        endpoint.MapPrometheusScrapingEndpoint();
        return app;
    }
}
