using Microsoft.AspNetCore.Mvc;

namespace Chaldea.Fate.RhoAias.Dashboard;

[ApiController]
[Route("api/dashboard/client")]
public class ClientController : ControllerBase
{
	private readonly IClientManager _clientManager;

	public ClientController(IClientManager clientManager)
	{
		_clientManager = clientManager;
	}

	public Task CreateAsync()
	{

	}

	public Task<IList<Client>> GetListAsync()
	{
		
	}
}