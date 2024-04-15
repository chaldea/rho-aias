using AutoMapper;

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
	}
}