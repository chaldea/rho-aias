using Microsoft.AspNetCore.Mvc;
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

public class UserProfileDto
{
	public Guid Id { get; set; }
	public string UserName { get; set; }
	public string Avatar { get; set; }
}

[Authorize(Roles = Role.User)]
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
			var token = await _tokenManager.CreateAsync(user.Id, Role.User, DateTime.UtcNow.AddHours(2));
			return new LoginResultDto
			{
				Status = "ok",
				Token = token
			};
		}

		return new LoginResultDto
		{
			Status = "error"
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