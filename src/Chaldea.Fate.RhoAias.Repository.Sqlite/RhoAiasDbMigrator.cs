using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chaldea.Fate.RhoAias.Repository.Sqlite;

public interface IDbMigrator
{
	Task MigrateAsync();
}

internal class RhoAiasDbMigrator : IDbMigrator
{
	private readonly IConfiguration _configuration;
	private readonly IServiceProvider _serviceProvider;

	public RhoAiasDbMigrator(IServiceProvider serviceProvider, IConfiguration configuration)
	{
		_serviceProvider = serviceProvider;
		_configuration = configuration;
	}

	public async Task MigrateAsync()
	{
		var str = $"{_configuration.GetConnectionString("RhoAias").TrimEnd(';')};";
		var match = Regex.Match(str, "(Filename|Data Source)=(.*?);");
		if (match.Groups.Count > 1)
		{
			var path = Path.GetDirectoryName(match.Groups[2].Value);
			if (path != null && !Directory.Exists(path)) Directory.CreateDirectory(path);
		}
		using (var scope = _serviceProvider.CreateScope())
		{
			await scope.ServiceProvider
				.GetRequiredService<RhoAiasDbContext>()
				.Database
				.MigrateAsync();
		}
	}
}