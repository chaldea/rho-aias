using Chaldea.Fate.RhoAias;
using Chaldea.Fate.RhoAias.Acme.LetsEncrypt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IRhoAiasConfigurationBuilder AddAhoAiasLetsEncrypt(this IRhoAiasConfigurationBuilder builder)
	{
		builder.Services.AddMemoryCache();
		builder.Services.AddOptions<RhoAiasLetsEncryptOptions>("RhoAias:Acme:LetsEncrypt");
		builder.Services.AddKeyedSingleton<IAcmeProvider, LetsEncryptAcmeProvider>("LetsEncrypt");
		return builder;
	}

	public static IRhoAiasApplicationBuilder UseAhoAiasLetsEncrypt(this IRhoAiasApplicationBuilder app)
	{
		app.EndpointRouteBuilder.MapGet("/.well-known/acme-challenge/{token}", async (IMemoryCache cache, HttpContext context, string token) =>
		{
			app.Logger.LogInformation($"acme-challenge: {token}");
			if (cache.TryGetValue(token, out var value))
			{
				context.Response.ContentType = "text/plain";
				await context.Response.WriteAsync(value.ToString());
				return;
			}
			context.Response.StatusCode = 404;
		});
		return app;
	}
}