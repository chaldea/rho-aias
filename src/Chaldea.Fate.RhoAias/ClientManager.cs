namespace Chaldea.Fate.RhoAias;

public interface IClientManager
{
    Task<Result> RegisterClientAsync(Client register);
    Task UnRegisterClientAsync(string connectionId);
    Task CreateClientAsync(Client entity);
    Task<Client?> GetClientAsync(Guid id);
    Task UpdateClientAsync(Client entity);
    Task RemoveClientAsync(Guid id);
    Task<List<Client>> GetClientListAsync();
    Task InitClientMetricsAsync();
    Task ResetClientStatusAsync();
    Task<Client?> GetClientAsync(string key);
}

internal class ClientManager : IClientManager
{
    private readonly IRepository<Client> _clientRepository;
    private readonly IForwarderManager _forwarderManager;
    private readonly IMetrics _metrics;

    public ClientManager(
        IRepository<Client> clientRepository,
        IForwarderManager forwarderManager,
        IMetrics metrics)
    {
        _clientRepository = clientRepository;
        _forwarderManager = forwarderManager;
        _metrics = metrics;
    }

    public async Task<Result> RegisterClientAsync(Client register)
    {
        var result = register.VersionCheck();
        if (!result.IsSuccess)
            return result;
        var client = await _clientRepository.GetAsync(x => x.Id == register.Id);
        if (client == null)
            return Result.Error(ErrorCode.InvalidClientToken.ToError());
        client.Update(register);
        await _clientRepository.UpdateAsync(client);
        _metrics.UpDownClientOnline(1);
        return Result.Success();
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
        if (await _clientRepository.AnyAsync(x => x.Name == entity.Name))
        {
            return;
        }
        entity.EnsureToken();
        await _clientRepository.InsertAsync(entity);
        _metrics.UpDownClientTotal(1);
    }

    public async Task UpdateClientAsync(Client entity)
    {
        if (await _clientRepository.AnyAsync(x => x.Name == entity.Name && x.Id != entity.Id))
        {
            return;
        }
        await _clientRepository.UpdateAsync(entity);
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

    public async Task<Client?> GetClientAsync(string token)
    {
        return await _clientRepository.GetAsync(x => x.Token == token);
    }

    public async Task<Client?> GetClientAsync(Guid id)
    {
        return await _clientRepository.GetAsync(x => x.Id == id);
    }
}