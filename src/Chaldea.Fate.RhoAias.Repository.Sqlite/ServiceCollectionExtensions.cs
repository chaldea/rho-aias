using Chaldea.Fate.RhoAias;
using Chaldea.Fate.RhoAias.Repository;
using Chaldea.Fate.RhoAias.Repository.Sqlite;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IRhoAiasConfigurationBuilder AddRhoAiasSqlite(this IRhoAiasConfigurationBuilder builder)
    {
        builder.Services.AddRhoAiasSqlite(builder.Configuration);
        return builder;
    }

    public static IRhoAiasApplicationBuilder UseRhoAiasSqlite(this IRhoAiasApplicationBuilder app)
    {
        app.ApplicationBuilder.UseRhoAiasSqlite();
        return app;
    }

    public static IServiceCollection AddRhoAiasSqlite(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<RhoAiasDbContext>(options => options.UseSqlite(configuration.GetRhoAiasConnectionString()));
        services.Replace(new ServiceDescriptor(typeof(IRepository<>), typeof(SqliteRepository<>), ServiceLifetime.Singleton));
        services.AddTransient<IDbMigrator, RhoAiasDbMigrator>();
        return services;
    }

    public static IApplicationBuilder UseRhoAiasSqlite(this IApplicationBuilder app)
    {
        var migrator = app.ApplicationServices.GetService<IDbMigrator>();
        migrator?.MigrateAsync().Wait();
        return app;
    }
}