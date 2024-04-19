namespace Chaldea.Fate.RhoAias;

public interface IProxyManager
{
	Task CreateProxyAsync(Proxy entity);
	Task RemoveProxyAsync(Guid id);
	Task<List<Proxy>> GetProxyListAsync();
	Task<int> CountProxyAsync();
}

internal class ProxyManager : IProxyManager
{
	private readonly IForwarderManager _forwarderManager;
	private readonly IRepository<Proxy> _proxyRepository;

	public ProxyManager(IRepository<Proxy> proxyRepository, IForwarderManager forwarderManager)
	{
		_proxyRepository = proxyRepository;
		_forwarderManager = forwarderManager;
	}

	public async Task CreateProxyAsync(Proxy entity)
	{
		entity.Id = Guid.NewGuid();
		await _proxyRepository.InsertAsync(entity);
		var proxies = await _proxyRepository.GetListAsync(x => x.Id == entity.Id && x.Client.Status, x => x.Client);
		if (proxies is { Count: > 0 }) _forwarderManager.Register(proxies);
	}

	public async Task RemoveProxyAsync(Guid id)
	{
		var proxies = await _proxyRepository.GetListAsync(x => x.Id == id);
		if (proxies is { Count: > 0 })
		{
			_forwarderManager.UnRegister(proxies);
			await _proxyRepository.DeleteManyAsync(proxies);
		}
	}

	public async Task<List<Proxy>> GetProxyListAsync()
	{
		var list = await _proxyRepository.GetListAsync(x => x.Client);
		return list;
	}

	public async Task<int> CountProxyAsync()
	{
		return await _proxyRepository.CountAsync();
	}
}