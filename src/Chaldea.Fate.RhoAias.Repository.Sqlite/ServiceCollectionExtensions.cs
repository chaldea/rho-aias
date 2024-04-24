using Chaldea.Fate.RhoAias;
using Chaldea.Fate.RhoAias.Repository;
using Chaldea.Fate.RhoAias.Repository.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IRhoAiasConfigurationBuilder AddRhoAiasSqlite(this IRhoAiasConfigurationBuilder builder)
	{
		var configuration = builder.Configuration;
		builder.Services.AddDbContextFactory<RhoAiasDbContext>(options => options.UseSqlite(configuration.GetRhoAiasConnectionString()));
		builder.Services.Replace(new ServiceDescriptor(typeof(IRepository<>), typeof(SqliteRepository<>), ServiceLifetime.Singleton));
		builder.Services.AddTransient<IDbMigrator, RhoAiasDbMigrator>();
		return builder;
	}

	public static IRhoAiasApplicationBuilder UseRhoAiasSqlite(this IRhoAiasApplicationBuilder app)
	{
		var migrator = app.Services.GetService<IDbMigrator>();
		migrator?.MigrateAsync().Wait();
		return app;
	}
}