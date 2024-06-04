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
        var configKey = "RhoAias:Metrics:Prometheus";
        var options = new RhoAiasPrometheusOptions();
        builder.Services.AddOptions<RhoAiasPrometheusOptions>(configKey);
        builder.Configuration.GetSection(configKey).Bind(options);
        builder.Services.AddOpenTelemetry().WithMetrics(b =>
        {
            b.AddPrometheusExporter();
            b.AddMeter(options.Meters);
        });
        return builder;
    }

    public static IRhoAiasApplicationBuilder UseRhoAiasPrometheus(this IRhoAiasApplicationBuilder builder)
    {
        builder.EndpointRouteBuilder.MapPrometheusScrapingEndpoint();
        return builder;
    }
}