using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        return Task.FromResult(new Token()
        {
            AccessToken = userId.ToString(),
            ExpiresIn = expires,
            TokenType = "Basic"
        });
    }
}

internal class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IClientManager _clientManager;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, 
        ILoggerFactory logger,
        UrlEncoder encoder,
        IClientManager clientManager) 
        : base(options, logger, encoder)
    {
        _clientManager = clientManager;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        try
        {
            var token = Request.Headers["Authorization"];
            var id = ParseId(token);
            var client = await _clientManager.GetClientAsync(id);
            if (client == null)
            {
                return AuthenticateResult.Fail("Invalid token.");
            }

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
                new Claim(ClaimTypes.Role, Role.Client)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }
    }

    private Guid ParseId(string token)
    {
        var id = Regex.Replace(token, @"(Bearer|Basic)\s", "");
        return Guid.Parse(id);
    }
}