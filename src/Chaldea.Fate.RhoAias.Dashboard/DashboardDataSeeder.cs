using Microsoft.Extensions.Options;

namespace Chaldea.Fate.RhoAias.Dashboard;

internal class DashboardDataSeeder : IDataSeeder
{
	private readonly IUserManager _userManager;
	private readonly IOptions<RhoAiasDashboardOptions> _options;

	public DashboardDataSeeder(IUserManager userManager, IOptions<RhoAiasDashboardOptions> options)
	{
		_userManager = userManager;
		_options = options;
	}

	public async Task SeedAsync()
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