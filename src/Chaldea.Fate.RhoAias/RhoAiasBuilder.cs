using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chaldea.Fate.RhoAias;

public interface IRhoAiasConfigurationBuilder
{
	IServiceCollection Services { get; }
	IConfiguration Configuration { get; }
}

internal class RhoAiasConfigurationBuilder : IRhoAiasConfigurationBuilder
{
	public RhoAiasConfigurationBuilder(IServiceCollection services, IConfiguration configuration)
	{
		Services = services;
		Configuration = configuration;
	}

	public IServiceCollection Services { get; }
	public IConfiguration Configuration { get; }
}

public interface IRhoAiasApplicationBuilder
{
	IServiceProvider Services { get; }
	IApplicationBuilder ApplicationBuilder { get; }
	IEndpointRouteBuilder EndpointRouteBuilder { get; }
}

internal class RhoAiasApplicationBuilder : IRhoAiasApplicationBuilder
{
	public RhoAiasApplicationBuilder(WebApplication app)
	{
		Services = app.Services;
		ApplicationBuilder = app;
		EndpointRouteBuilder = app;
	}

	public IServiceProvider Services { get; }
	public IApplicationBuilder ApplicationBuilder { get; }
	public IEndpointRouteBuilder EndpointRouteBuilder { get; }
}