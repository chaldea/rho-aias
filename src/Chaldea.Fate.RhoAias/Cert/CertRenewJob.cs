using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chaldea.Fate.RhoAias;

internal class CertRenewJob : BackgroundService
{
    private readonly ILogger<CertRenewJob> _logger;
    private readonly ICertManager _certManager;
    private readonly IOptions<CertManagerOptions> _options;

    public CertRenewJob(
        ILogger<CertRenewJob> logger,
        ICertManager certManager,
        IOptions<CertManagerOptions> options)
    {
        _logger = logger;
        _certManager = certManager;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.Value.AutoRenewCerts)
        {
            using PeriodicTimer timer = new(TimeSpan.FromHours(1));
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    try
                    {
                        await _certManager.RenewAllAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("CertRenewJob Service is stopping.");
            }
        }
    }
}
