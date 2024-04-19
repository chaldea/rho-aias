using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Chaldea.Fate.RhoAias.Authentication.JwtBearer;

internal class JwtBearerTokenManager : ITokenManager
{
	private readonly IOptions<RhoAiasJwtOptions> _options;

	public JwtBearerTokenManager(IOptions<RhoAiasJwtOptions> options)
	{
		_options = options;
	}

	public Task<string> CreateAsync(Guid userId, string role, DateTime expires)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.Role, role),
			new(ClaimTypes.NameIdentifier, userId.ToString())
		};

		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_options.Value.Secret);
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Expires = expires,
			Audience = _options.Value.Audience,
			Issuer = _options.Value.Audience,
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};
		var token = tokenHandler.CreateToken(tokenDescriptor);
		var tokenString = tokenHandler.WriteToken(token);
		return Task.FromResult(tokenString);
	}
}