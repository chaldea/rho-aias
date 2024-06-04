using System.Text.Json;

namespace Chaldea.Fate.RhoAias;

public class DnsProvider
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Provider { get; set; } = default!;
    public string Config { get; set; } = default!;
    public string? LatestRecordId { get; set; }

    public T? GetConfig<T>()
    {
        return JsonSerializer.Deserialize<T>(Config);
    }
}

public interface IDnsProvider
{
    Task<string?> CreateAsync(DnsProvider provider, string domain, string value);
    Task<bool> RemoveAsync(DnsProvider provider, string id);
    Task<string?> ExistsAsync(DnsProvider provider, string domain);
}