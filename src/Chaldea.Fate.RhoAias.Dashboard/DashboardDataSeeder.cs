using Microsoft.Extensions.Options;

namespace Chaldea.Fate.RhoAias.Dashboard;

internal class DashboardDataSeeder : IDataSeeder
{
	private readonly IOptions<RhoAiasDashboardOptions> _options;
	private readonly IClientManager _clientManager;
	private readonly IUserManager _userManager;

	public DashboardDataSeeder(
		IClientManager clientManager, 
		IUserManager userManager,
		IOptions<RhoAiasDashboardOptions> options)
	{
		_clientManager = clientManager;
		_userManager = userManager;
		_options = options;
	}

	public async Task SeedAsync()
	{
		// create default user
		var user = new User
		{
			Id = Guid.NewGuid(),
			UserName = _options.Value.UserName
		};
		user.HashPassword(_options.Value.Password);
		await _userManager.CreateAsync(user);

		if (_options.Value.CreateDefaultClient)
		{
			var client = new Client()
			{
				Id = Guid.Parse("7e89c9be-4278-4acf-b5fb-a1ec9a67452c"),
				Name = "default",
				Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoiY2xpZW50IiwibmFtZWlkIjoiN2U4OWM5YmUtNDI3OC00YWNmLWI1ZmItYTFlYzlhNjc0NTJjIiwibmJmIjoxNzEzOTQ2ODE5LCJleHAiOjIwMjk0Nzk2MTksImlhdCI6MTcxMzk0NjgxOSwiaXNzIjoiUmhvQWlhcyIsImF1ZCI6IlJob0FpYXMifQ.uBWI6GSf74JzdKSdisXtxwUkVEjyqP0JnKCJ2NYhGi4"
			};
			await _clientManager.CreateDefaultClientAsync(client);
		}
	}
}