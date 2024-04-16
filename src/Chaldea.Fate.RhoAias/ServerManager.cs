namespace Chaldea.Fate.RhoAias;

public interface IServerManager
{
	Task<bool> RegisterClientAsync(Client register);
	Task UnRegisterClientAsync(string connectionId);
	Task CreateClientAsync(Client entity);
	Task RemoveClientAsync(Guid id);
	Task<List<Client>> GetClientListAsync();
	Task CreateProxyAsync(Proxy entity);
	Task RemoveProxyAsync(Guid id);
	Task<List<Proxy>> GetProxyListAsync();
}

internal class ServerManager : IServerManager
{
	private readonly IRepository<Client> _clientRepository;
	private readonly IRepository<Proxy> _proxyRepository;
	private readonly IForwarderManager _forwarderManager;
	private readonly ITokenManager _tokenManager;

	public ServerManager(
		IRepository<Client> clientRepository,
		IRepository<Proxy> proxyRepository,
		IForwarderManager forwarderManager,
		ITokenManager tokenManager)
	{
		_clientRepository = clientRepository;
		_proxyRepository = proxyRepository;
		_forwarderManager = forwarderManager;
		_tokenManager = tokenManager;
	}

	public async Task<bool> RegisterClientAsync(Client register)
	{
		var client = await _clientRepository.GetAsync(x => x.Id == register.Id, y => y.Proxies);
		if (client == null) return false;
		client.Endpoint = register.Endpoint;
		client.ConnectionId = register.ConnectionId;
		client.Status = register.Status;
		client.Version = register.Version;
		// if client has proxies, replace to server.
		if (register.Proxies is { Count: > 0 })
		{
			client.Proxies?.Clear();
			client.Proxies = register.Proxies;
		}
		// register all proxies
		if (client.Proxies is { Count: > 0 })
		{
			_forwarderManager.Register(client.Proxies.ToList());
		}
		await _clientRepository.UpdateAsync(client);
		return true;
	}

	public async Task UnRegisterClientAsync(string connectionId)
	{
		// get client
		var client = await _clientRepository.GetAsync(x => x.ConnectionId == connectionId, y => y.Proxies);
		if (client == null) return;
		// update status
		client.Status = false;
		// unregister proxies
		if (client.Proxies is { Count: > 0 })
		{
			_forwarderManager.UnRegister(client.Proxies.ToList());
		}
		await _clientRepository.UpdateAsync(client);
	}

	public async Task CreateClientAsync(Client entity)
	{
		var cid = Guid.NewGuid();
		entity.Id = cid;
		entity.Token = await _tokenManager.CreateAsync(cid);
		await _clientRepository.InsertAsync(entity);
	}

	public async Task RemoveClientAsync(Guid id)
	{
		// Cascade Delete
		var entity = await _clientRepository.GetAsync(x => x.Id == id, x => x.Proxies);
		if (entity != null)
		{
			await _clientRepository.DeleteAsync(entity);
		}
	}

	public async Task<List<Client>> GetClientListAsync()
	{
		var clients = await _clientRepository.GetListAsync();
		return clients;
	}

	public async Task CreateProxyAsync(Proxy entity)
	{
		entity.Id = Guid.NewGuid();
		await _proxyRepository.InsertAsync(entity);
		var proxies = await _proxyRepository.GetListAsync(x => x.Id == entity.Id && x.Client.Status, x => x.Client);
		if (proxies is { Count: > 0 })
		{
			_forwarderManager.Register(proxies);
		}
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
}