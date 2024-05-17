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
		await CreateDefaultUserAsync();
		await CreateDefaultClientAsync();
	}

	private async Task CreateDefaultUserAsync()
	{
		if (_options.Value.CreateDefaultUser)
		{
			var user = new User
			{
				Id = Guid.NewGuid(),
				UserName = _options.Value.UserName
			};
			user.HashPassword(_options.Value.Password);
			await _userManager.CreateAsync(user);
		}
	}

	private async Task CreateDefaultClientAsync()
	{
		if (_options.Value.CreateDefaultClient)
		{
			var client = new Client()
			{
				Id = Guid.Parse("375FC436-4195-4839-B7E7-7FDF03AA7FB1"),
				Name = "default",
				Token = "NsRfN5VBOUi353fA6psQ",
			};
			await _clientManager.CreateDefaultClientAsync(client);

			var ingress = new Client()
			{
				Id = Guid.Parse("64516AAE-FA74-479C-B4EA-562094BA821F"),
				Name = "ingress",
				Token = "rmpRZHT6nEe06lYglLqCHw",
			};
			await _clientManager.CreateDefaultClientAsync(ingress);
		}
	}
}