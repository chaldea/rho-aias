namespace Chaldea.Fate.RhoAias;

public interface IDnsProviderManager
{
    Task CreateDnsProviderAsync(DnsProvider entity);
    Task<List<DnsProvider>> GetDnsProviderListAsync();
    Task RemoveDnsProviderAsync(Guid id);
}

internal class DnsProviderManager : IDnsProviderManager
{
    private readonly IRepository<DnsProvider> _dnsProviderRepository;

    public DnsProviderManager(IRepository<DnsProvider> dnsProviderRepository)
    {
        _dnsProviderRepository = dnsProviderRepository;
    }

    public async Task CreateDnsProviderAsync(DnsProvider entity)
    {
        entity.Id = Guid.NewGuid();
        await _dnsProviderRepository.InsertAsync(entity);
    }

    public async Task<List<DnsProvider>> GetDnsProviderListAsync()
    {
        var data = await _dnsProviderRepository.GetListAsync();
        return data;
    }

    public async Task RemoveDnsProviderAsync(Guid id)
    {
        var cert = await _dnsProviderRepository.GetAsync(x => x.Id == id);
        if (cert == null) return;
        await _dnsProviderRepository.DeleteAsync(cert);
    }
}