using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace Chaldea.Fate.RhoAias.Dashboard;

public class LoginDto
{
	public string UserName { get; set; }
	public string Password { get; set; }
	public string Type { get; set; }
}

public class LoginResultDto
{
	public string Status { get; set; }
	public string Token { get; set; }
	public string Type { get; set; }
}

public class User
{
	public Guid Id { get; set; }
	public string UserName { get; set; }
	public string Password { get; set; }

	public bool VerifyPassword(string password)
	{
		return PasswordHasher.VerifyPassword(password, Password);
	}
}

public class UserProfileDto
{
	public Guid Id { get; set; }
	public string UserName { get; set; }
	public string Avatar { get; set; }
}

public class PasswordHasher
{
	private const int SaltSize = 16; // 盐值的大小，以字节为单位
	private const int HashSize = 20; // 哈希值的大小，以字节为单位
	private const int Iterations = 10000; // 迭代次数

	public static string HashPassword(string password)
	{
		// 生成随机的盐值
		byte[] salt;
		new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

		// 使用 PBKDF2 算法进行密码哈希
		var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
		byte[] hash = pbkdf2.GetBytes(HashSize);

		// 将盐值和哈希值合并存储
		byte[] hashBytes = new byte[SaltSize + HashSize];
		Array.Copy(salt, 0, hashBytes, 0, SaltSize);
		Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

		// 将字节数组转换为 Base64 字符串存储
		return Convert.ToBase64String(hashBytes);
	}

	public static bool VerifyPassword(string password, string hashedPassword)
	{
		// 从 Base64 字符串解析盐值和哈希值
		byte[] hashBytes = Convert.FromBase64String(hashedPassword);
		byte[] salt = new byte[SaltSize];
		Array.Copy(hashBytes, 0, salt, 0, SaltSize);
		byte[] hash = new byte[HashSize];
		Array.Copy(hashBytes, SaltSize, hash, 0, HashSize);

		// 使用相同的盐值和迭代次数计算密码的哈希值
		var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
		byte[] testHash = pbkdf2.GetBytes(HashSize);

		// 比较两个哈希值是否相等
		for (int i = 0; i < HashSize; i++)
		{
			if (hash[i] != testHash[i])
				return false;
		}
		return true;
	}
}

public interface IUserManager
{
	Task<User?> GetAsync(Guid id);
	Task<User?> GetAsync(string name);
}

public class UserManager : IUserManager
{
	private List<User> _users = new List<User>();
	public UserManager()
	{
		_users.Add(new User
			{ Id = Guid.NewGuid(), UserName = "admin", Password = PasswordHasher.HashPassword("123456Aa") });
	}

	public async Task<User?> GetAsync(string name)
	{
		return _users.FirstOrDefault(x => x.UserName == name);
	}

	public async Task<User?> GetAsync(Guid id)
	{
		return _users.FirstOrDefault(x => x.Id == id);
	}
}

[Authorize]
[ApiController]
[Route("api/dashboard/user")]
public class UserController : ControllerBase
{
	private readonly ITokenManager _tokenManager;
	private readonly IUserManager _userManager;
	private readonly IMapper _mapper;

	public UserController(ITokenManager tokenManager, IUserManager userManager, IMapper mapper)
	{
		_tokenManager = tokenManager;
		_userManager = userManager;
		_mapper = mapper;
	}

	[HttpPost]
	[Route("login")]
	[AllowAnonymous]
	public async Task<LoginResultDto> LoginAsync(LoginDto dto)
	{
		var user = await _userManager.GetAsync(dto.UserName);
		if (user == null)
		{
			return new LoginResultDto
			{
				Status = "error"
			};
		}

		if (user.VerifyPassword(dto.Password))
		{
			var token = await _tokenManager.CreateAsync(user.Id);
			return new LoginResultDto
			{
				Status = "ok",
				Token = token
			};
		}

		return new LoginResultDto
		{
			Status = "errror"
		};
	}

	[HttpGet]
	[Route("profile")]
	public async Task<UserProfileDto> GetProfileAsync()
	{
		var id = User.UserId();
		var user = await _userManager.GetAsync(id);
		return _mapper.Map<User, UserProfileDto>(user);
	}
}