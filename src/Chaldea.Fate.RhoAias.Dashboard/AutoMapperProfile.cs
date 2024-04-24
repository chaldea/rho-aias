using AutoMapper;
using Chaldea.Fate.RhoAias.Dashboard.Controllers;

namespace Chaldea.Fate.RhoAias.Dashboard;

internal class AutoMapperProfile : Profile
{
	public AutoMapperProfile()
	{
		CreateMap<ClientCreateDto, Client>();
		CreateMap<Client, ClientDto>();
		CreateMap<Proxy, ProxyDto>();
		CreateMap<ProxyDto, Proxy>();
		CreateMap<User, UserProfileDto>();
		CreateMap<Cert, CertDto>();
		CreateMap<CertCreateDto, Cert>();
		CreateMap<DnsProviderDto, DnsProvider>();
		CreateMap<DnsProvider, DnsProviderDto>();
	}
}