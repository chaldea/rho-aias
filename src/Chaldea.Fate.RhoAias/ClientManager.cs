namespace Chaldea.Fate.RhoAias;

public interface IClientManager
{
	Task<bool> RegisterClientAsync(Client register);
	Task UnRegisterClientAsync(string connectionId);
	Task CreateClientAsync(Client entity);
	Task RemoveClientAsync(Guid id);
	Task<List<Client>> GetClientListAsync();
	Task InitClientMetricsAsync();
	Task ResetClientStatusAsync();
}

internal class ClientManager : IClientManager
{
	private readonly IRepository<Client> _clientRepository;
	private readonly IForwarderManager _forwarderManager;
	private readonly ITokenManager _tokenManager;
	private readonly IMetrics _metrics;

	public ClientManager(
		IRepository<Client> clientRepository,
		IForwarderManager forwarderManager,
		ITokenManager tokenManager,
		IMetrics metrics)
	{
		_clientRepository = clientRepository;
		_forwarderManager = forwarderManager;
		_tokenManager = tokenManager;
		_metrics = metrics;
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
		if (client.Proxies is { Count: > 0 }) _forwarderManager.Register(client.Proxies.ToList());
		await _clientRepository.UpdateAsync(client);
		_metrics.UpDownClientOnline(1);
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
		if (client.Proxies is { Count: > 0 }) _forwarderManager.UnRegister(client.Proxies.ToList());
		await _clientRepository.UpdateAsync(client);
		_metrics.UpDownClientOnline(-1);
	}

	public async Task CreateClientAsync(Client entity)
	{
		var cid = Guid.NewGuid();
		entity.Id = cid;
		entity.Token = await _tokenManager.CreateAsync(cid, Role.Client, DateTime.UtcNow.AddYears(10));
		await _clientRepository.InsertAsync(entity);
		_metrics.UpDownClientTotal(1);
	}

	public async Task RemoveClientAsync(Guid id)
	{
		// Cascade Delete
		var entity = await _clientRepository.GetAsync(x => x.Id == id, x => x.Proxies);
		if (entity != null) await _clientRepository.DeleteAsync(entity);
		_metrics.UpDownClientTotal(-1);
	}

	public async Task<List<Client>> GetClientListAsync()
	{
		var clients = await _clientRepository.GetListAsync();
		return clients;
	}

	public async Task InitClientMetricsAsync()
	{
		var total = await _clientRepository.CountAsync();
		_metrics.UpDownClientTotal(total);
	}

	public async Task ResetClientStatusAsync()
	{
		var clients = await _clientRepository.GetListAsync(x => x.Status);
		if (clients.Count <= 0) return;
		foreach (var client in clients)
		{
			client.Status = false;
		}
		await _clientRepository.UpdateManyAsync(clients);
	}
}