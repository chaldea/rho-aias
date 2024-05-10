namespace Chaldea.Fate.RhoAias;

public interface IProxyManager
{
	Task CreateProxyAsync(Proxy entity);
	Task RemoveProxyAsync(Guid id);
	Task<List<Proxy>> GetProxyListAsync();
	Task<int> CountProxyAsync();
	Task UpdateProxyListAsync(Guid clientId, List<Proxy> proxies);
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
		entity.UpdateLocalIp();
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

	public async Task UpdateProxyListAsync(Guid clientId, List<Proxy>? proxies)
	{
		// serverProxies is from server db.
		var serverProxies = await _proxyRepository.GetListAsync(x => x.ClientId == clientId, y => y.Client);
		var allProxies = serverProxies;
		var client = serverProxies.FirstOrDefault()?.Client;
		if (proxies != null)
		{
			var update = new List<Proxy>();
			var insert = new List<Proxy>();
			// proxies is from client config.(eg: appsettings, or k8s-ingress)
			foreach (var proxy in proxies)
			{
				proxy.ClientId = clientId;
				proxy.Client = client;
				var exists = serverProxies.FirstOrDefault(x => x.Name == proxy.Name);
				if (exists != null)
				{
					exists.Update(proxy);
					exists.UpdateLocalIp();
					update.Add(exists); // update from client
				}
				else
				{
					proxy.UpdateLocalIp();
					insert.Add(proxy); // add new from client
				}
			}

			await _proxyRepository.UpdateManyAsync(update);
			await _proxyRepository.InsertManyAsync(insert);
			allProxies = serverProxies.Concat(insert).ToList();
		}
		if (allProxies is { Count: > 0 }) _forwarderManager.Register(allProxies);
	}
}