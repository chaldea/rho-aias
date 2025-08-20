using Chaldea.Fate.RhoAias;
using Chaldea.Fate.RhoAias.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IRhoAiasConfigurationBuilder AddRhoAiasDashboard(this IRhoAiasConfigurationBuilder builder)
    {
        builder.Services.AddRhoAiasDashboard(builder.Configuration);
        return builder;
    }

    public static IRhoAiasApplicationBuilder UseRhoAiasDashboard(this IRhoAiasApplicationBuilder builder)
    {
        builder.ApplicationBuilder.UseRhoAiasDashboard();
        return builder;
    }

    public static IServiceCollection AddRhoAiasDashboard(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RhoAiasDashboardOptions>(configuration.GetSection("RhoAias:Dashboard"));
        services.AddSingleton<IDataSeeder, DashboardDataSeeder>();
        services.AddControllers();
        services.AddAutoMapper(config => { config.AddProfile<AutoMapperProfile>(); });
        services.AddSpaStaticFiles(options => { options.RootPath = "wwwroot/dashboard"; });
        return services;
    }

    public static IApplicationBuilder UseRhoAiasDashboard(this IApplicationBuilder app)
    {
        app.UseSpaStaticFiles();
        app.UseDashboardSpa(spa => { spa.Options.DefaultPage = "/index.html"; });
        var endpoint = app.GetEndpointRoute();
        endpoint.MapControllers();
        return app;
    }
}
