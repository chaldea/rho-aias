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