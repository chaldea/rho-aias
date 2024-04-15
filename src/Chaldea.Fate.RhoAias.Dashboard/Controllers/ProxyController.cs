using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chaldea.Fate.RhoAias.Dashboard;

public class ProxyDto
{
	public Guid? Id { get; set; }
	public string Name { get; set; }
	public ProxyType Type { get; set; }
	public string LocalIP { get; set; }
	public int LocalPort { get; set; }
	public int? RemotePort { get; set; }
	public string? Path { get; set; }
	public string[]? Hosts { get; set; }
	public Guid? ClientId { get; set; }
	public ClientDto? Client { get; set; }
}

[Authorize]
[ApiController]
[Route("api/dashboard/proxy")]
public class ProxyController : ControllerBase
{
	private readonly IServerManager _serverManager;
	private readonly IMapper _mapper;

	public ProxyController(
		IServerManager serverManager,
		IMapper mapper)
	{
		_serverManager = serverManager;
		_mapper = mapper;
	}

	[HttpPut]
	[Route("create")]
	public async Task CreateAsync(ProxyDto dto)
	{
		var entity = _mapper.Map<ProxyDto, Proxy>(dto);
		await _serverManager.CreateProxyAsync(entity);
	}

	[HttpDelete]
	[Route("remove")]
	public async Task RemoveAsync(Guid id)
	{
		await _serverManager.RemoveProxyAsync(id);
	}

	[HttpGet]
	[Route("list")]
	public async Task<List<ProxyDto>> GetListAsync()
	{
		var list = await _serverManager.GetProxyListAsync();
		return _mapper.Map<List<Proxy>, List<ProxyDto>>(list);
	}
}