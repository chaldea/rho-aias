using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chaldea.Fate.RhoAias.Dashboard;

public class CertCreateDto
{
    public int CertType { get; set; }
    public string Domain { get; set; }
    public DateTime? Expires { get; set; }
    public string Issuer { get; set; }
    public string Email { get; set; }
    public Guid? DnsProviderId { get; set; }
}

public class CertDto
{
    public Guid Id { get; set; }
    public int CertType { get; set; }
    public string Domain { get; set; }
    public DateTime? Expires { get; set; }
    public string Issuer { get; set; }
    public string Email { get; set; }
    public AcmeStatus Status { get; set; }
}

[Authorize(Roles = Role.User)]
[ApiController]
[Route("api/dashboard/cert")]
public class CertController : ControllerBase
{
    private readonly ICertManager _certManager;
    private readonly IMapper _mapper;

    public CertController(ICertManager certManager, IMapper mapper)
    {
        _certManager = certManager;
        _mapper = mapper;
    }

    [HttpPut]
    [Route("create")]
    public async Task CreateAsync(CertCreateDto dto)
    {
        var entity = _mapper.Map<CertCreateDto, Cert>(dto);
        await _certManager.CreateCertAsync(entity);
    }

    [HttpDelete]
    [Route("remove")]
    public async Task RemoveAsync(Guid id)
    {
        await _certManager.RemoveCertAsync(id);
    }

    [HttpGet]
    [Route("list")]
    public async Task<List<CertDto>> GetListAsync()
    {
        var list = await _certManager.GetCertListAsync();
        return _mapper.Map<List<Cert>, List<CertDto>>(list);
    }
}