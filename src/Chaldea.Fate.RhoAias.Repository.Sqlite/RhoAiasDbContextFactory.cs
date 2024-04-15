using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Chaldea.Fate.RhoAias.Repository.Sqlite
{
	internal class RhoAiasDbContextFactory : IDesignTimeDbContextFactory<RhoAiasDbContext>
	{
		public RhoAiasDbContext CreateDbContext(string[] args)
		{
			var configuration = BuildConfiguration();
			var builder = new DbContextOptionsBuilder<RhoAiasDbContext>()
				.UseSqlite(configuration.GetConnectionString("RhoAias"));

			return new RhoAiasDbContext(builder.Options);
		}

		private static IConfigurationRoot BuildConfiguration()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
				.AddJsonFile("appsettings.json", optional: false);

			return builder.Build();
		}
	}
}
