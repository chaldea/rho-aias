namespace Chaldea.Fate.RhoAias;

public interface IUserManager
{
	Task CreateAsync(User user);
	Task<User?> GetAsync(Guid id);
	Task<User?> GetAsync(string name);
}

internal class UserManager : IUserManager
{
	private readonly IRepository<User> _userRepository;

	public UserManager(IRepository<User> userRepository)
	{
		_userRepository = userRepository;
	}

	public async Task CreateAsync(User user)
	{
		if (await _userRepository.AnyAsync(x => x.UserName == user.UserName))
		{
			return;
		}

		await _userRepository.InsertAsync(user);
	}

	public async Task<User?> GetAsync(string name)
	{
		return await _userRepository.GetAsync(x => x.UserName == name);
	}

	public async Task<User?> GetAsync(Guid id)
	{
		return await _userRepository.GetAsync(x => x.Id == id);
	}
}