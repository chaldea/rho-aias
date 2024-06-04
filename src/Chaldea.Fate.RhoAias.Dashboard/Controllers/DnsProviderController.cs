using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chaldea.Fate.RhoAias.Dashboard.Controllers;

public class DnsProviderDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; }
    public string Provider { get; set; }
    public string Config { get; set; }
}

[Authorize(Roles = Role.User)]
[ApiController]
[Route("api/dashboard/dns-provider")]
public class DnsProviderController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IDnsProviderManager _dnsProviderManager;

    public DnsProviderController(IMapper mapper, IDnsProviderManager dnsProviderManager)
    {
        _mapper = mapper;
        _dnsProviderManager = dnsProviderManager;
    }

    [HttpPut]
    [Route("create")]
    public async Task CreateAsync(DnsProviderDto dto)
    {
        var entity = _mapper.Map<DnsProviderDto, DnsProvider>(dto);
        entity.Id = Guid.NewGuid();
        await _dnsProviderManager.CreateDnsProviderAsync(entity);
    }

    [HttpDelete]
    [Route("remove")]
    public async Task RemoveAsync(Guid id)
    {
        await _dnsProviderManager.RemoveDnsProviderAsync(id);
    }

    [HttpGet]
    [Route("list")]
    public async Task<List<DnsProviderDto>> GetListAsync()
    {
        var list = await _dnsProviderManager.GetDnsProviderListAsync();
        return _mapper.Map<List<DnsProvider>, List<DnsProviderDto>>(list);
    }
}