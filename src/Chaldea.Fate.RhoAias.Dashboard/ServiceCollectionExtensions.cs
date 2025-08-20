using Chaldea.Fate.RhoAias;
using Chaldea.Fate.RhoAias.Dashboard;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IRhoAiasConfigurationBuilder AddRhoAiasDashboard(this IRhoAiasConfigurationBuilder builder)
    {
        builder.Services.Configure<RhoAiasDashboardOptions>(builder.Configuration.GetSection("RhoAias:Dashboard"));
        builder.Services.AddSingleton<IDataSeeder, DashboardDataSeeder>();
        builder.Services.AddControllers();
        builder.Services.AddAutoMapper(config => { config.AddProfile<AutoMapperProfile>(); });
        builder.Services.AddSpaStaticFiles(options => { options.RootPath = "wwwroot/dashboard"; });
        return builder;
    }

    public static IRhoAiasApplicationBuilder UseRhoAiasDashboard(this IRhoAiasApplicationBuilder builder)
    {
        var app = builder.ApplicationBuilder;
        var endpoint = builder.EndpointRouteBuilder;
        app.UseSpaStaticFiles();
        app.UseDashboardSpa(spa => { spa.Options.DefaultPage = "/index.html"; });
        endpoint.MapControllers();
        return builder;
    }
}
