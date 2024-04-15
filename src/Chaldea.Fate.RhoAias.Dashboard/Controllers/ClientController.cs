using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chaldea.Fate.RhoAias.Dashboard;

public record ClientCreateDto(string Name);

public class ClientDto
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string? Version { get; set; }
	public string? Token { get; set; }
	public string? Endpoint { get; set; }
	public string? ConnectionId { get; set; }
	public bool Status { get; set; }
}

[Authorize]
[ApiController]
[Route("api/dashboard/client")]
public class ClientController : ControllerBase
{
	private readonly IServerManager _serverManager;
	private readonly IMapper _mapper;

	public ClientController(IServerManager serverManager, IMapper mapper)
	{
		_serverManager = serverManager;
		_mapper = mapper;
	}

	[HttpPut]
	[Route("create")]
	public async Task CreateAsync(ClientCreateDto dto)
	{
		var entity = _mapper.Map<ClientCreateDto, Client>(dto);
		await _serverManager.CreateClientAsync(entity);
	}

	[HttpDelete]
	[Route("remove")]
	public async Task RemoveAsync(Guid id)
	{
		await _serverManager.RemoveClientAsync(id);
	}

	[HttpGet]
	[Route("list")]
	public async Task<ICollection<ClientDto>> GetListAsync()
	{
		var clients = await _serverManager.GetClientListAsync();
		return _mapper.Map<List<Client>, ICollection<ClientDto>>(clients);
	}
}