using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Chaldea.Fate.RhoAias;

public interface ITokenManager
{
    Task<Token> CreateAsync(Guid userId, string role, int expires);
}

internal class TokenManager : ITokenManager
{
    public Task<Token> CreateAsync(Guid userId, string role, int expires)
    {
        var token = Regex.Replace(Convert.ToBase64String(userId.ToByteArray()), "[/+=]", "");
        return Task.FromResult(new Token()
        {
            AccessToken = token,
            ExpiresIn = expires,
            TokenType = "Base"
        });
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