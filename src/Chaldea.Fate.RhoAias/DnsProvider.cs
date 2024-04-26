using System.Text.Json;

namespace Chaldea.Fate.RhoAias;

public class DnsProvider
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string Provider { get; set; }
	public string Config { get; set; }
	public string? LatestRecordId { get; set; }

	public T GetConfig<T>()
	{
		return JsonSerializer.Deserialize<T>(Config);
	}
}

public interface IDnsProvider
{
	Task<string?> CreateAsync(DnsProvider provider, string domain, string value);
	Task<bool> RemoveAsync(DnsProvider provider, string id);
}