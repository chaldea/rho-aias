using System.Configuration;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias.Repository.Sqlite;

public interface IDbMigrator
{
    Task MigrateAsync();
}

internal class RhoAiasDbMigrator : IDbMigrator
{
    private readonly IConfiguration _configuration;
    private readonly IEnumerable<IDataSeeder> _dataSeeders;
    private readonly ILogger<RhoAiasDbMigrator> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RhoAiasDbMigrator(
        ILogger<RhoAiasDbMigrator> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        IEnumerable<IDataSeeder> dataSeeders)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _dataSeeders = dataSeeders;
    }

    public async Task MigrateAsync()
    {
        _logger.LogInformation("Migrate database.");
        var str = $"{_configuration.GetRhoAiasConnectionString().TrimEnd(';')};";
        var match = Regex.Match(str, "(Filename|Data Source)=(.*?);");
        if (match.Groups.Count > 1)
        {
            var path = Path.GetDirectoryName(match.Groups[2].Value);
            if (path != null && !Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RhoAiasDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        foreach (var dataSeeder in _dataSeeders)
        {
            _logger.LogInformation($"Exec dataseed: {dataSeeder.GetType().Name}");
            await dataSeeder.SeedAsync();
        }
    }
}