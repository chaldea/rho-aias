using System.Text;
using Chaldea.Fate.RhoAias;
using Chaldea.Fate.RhoAias.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IRhoAiasConfigurationBuilder AddAhoAiasJwtBearerAuthentication(this IRhoAiasConfigurationBuilder builder)
	{
		var services = builder.Services;
		var configKey = "RhoAias:Authentication:Jwt";
		var options = new RhoAiasJwtOptions();
		services.AddOptions<RhoAiasJwtOptions>(configKey);
		builder.Configuration.GetSection(configKey).Bind(options);
		services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddCookie(cfg => cfg.SlidingExpiration = true)
			.AddJwtBearer(x =>
			{
				x.RequireHttpsMetadata = true;
				x.SaveToken = true;
				x.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.Secret)),
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidIssuer = options.Issuer,
					ValidAudience = options.Audience
				};
				x.Events = new JwtBearerEvents()
				{
					OnMessageReceived = context =>
					{
						var accessToken = context.Request.Query["access_token"];
						var path = context.HttpContext.Request.Path;
						if (!string.IsNullOrEmpty(accessToken) &&
						    (path.StartsWithSegments("/userhub")))
						{
							context.Token = accessToken;
						}

						return Task.CompletedTask;
					}
				};
			});
		services.Replace(new ServiceDescriptor(typeof(ITokenManager), typeof(JwtBearerTokenManager), ServiceLifetime.Singleton));
		return builder;
	}
}