using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chaldea.Fate.RhoAias.Dashboard;

public class ProxyDto
{
	public Guid? Id { get; set; }
	public string Name { get; set; }
	public ProxyType Type { get; set; }
	public string? LocalIP { get; set; }
	public int? LocalPort { get; set; }
	public int? RemotePort { get; set; }
	public string? Path { get; set; }
	public string[]? Hosts { get; set; }
	public string? Destination { get; set; }
	public Guid? ClientId { get; set; }
	public ClientDto? Client { get; set; }
	public bool? Disabled { get; set; }
}

public class ProxyStatusDto
{
	public Guid Id { get; set; }
	public bool Disabled { get; set; }
}

[Authorize(Roles = Role.User)]
[ApiController]
[Route("api/dashboard/proxy")]
public class ProxyController : ControllerBase
{
	private readonly IProxyManager _proxyManager;
	private readonly IMapper _mapper;

	public ProxyController(
		IProxyManager proxyManager,
		IMapper mapper)
	{
		_proxyManager = proxyManager;
		_mapper = mapper;
	}

	[HttpPut]
	[Route("create")]
	public async Task CreateAsync(ProxyDto dto)
	{
		var entity = _mapper.Map<ProxyDto, Proxy>(dto);
		await _proxyManager.CreateProxyAsync(entity);
	}

	[HttpPost]
	[Route("update")]
	public async Task UpdateAsync(ProxyDto dto)
	{
		var entity = _mapper.Map<ProxyDto, Proxy>(dto);
		await _proxyManager.UpdateProxyAsync(entity);
	}

	[HttpDelete]
	[Route("remove")]
	public async Task RemoveAsync(Guid id)
	{
		await _proxyManager.RemoveProxyAsync(id);
	}

	[HttpGet]
	[Route("list")]
	public async Task<List<ProxyDto>> GetListAsync()
	{
		var list = await _proxyManager.GetProxyListAsync();
		return _mapper.Map<List<Proxy>, List<ProxyDto>>(list);
	}

	[HttpPost]
	[Route("update-status")]
	public async Task DisableAsync(ProxyStatusDto dto)
	{
		await _proxyManager.UpdateStatusAsync(dto.Id, dto.Disabled);
	}
}