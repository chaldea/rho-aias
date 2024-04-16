using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Chaldea.Fate.RhoAias;

public interface ITokenManager
{
	Task<string> CreateAsync(Guid userId);
}

internal class TokenManager : ITokenManager
{
	public Task<string> CreateAsync(Guid userId)
	{
		var token = Regex.Replace(Convert.ToBase64String(userId.ToByteArray()), "[/+=]", "");
		return Task.FromResult(token);
	}
}

public static class Extensions
{
	public static Guid UserId(this ClaimsPrincipal user)
	{
		var sub = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
		if (sub != null)
		{
			return Guid.Parse(sub.Value);
		}
		return Guid.Empty;
	}
}