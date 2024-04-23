using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
	IConfiguration Configuration { get; }
	IServiceProvider Services { get; }
	IApplicationBuilder ApplicationBuilder { get; }
	IEndpointRouteBuilder EndpointRouteBuilder { get; }
	ILogger Logger { get; }
}

internal class RhoAiasApplicationBuilder : IRhoAiasApplicationBuilder
{
	public RhoAiasApplicationBuilder(WebApplication app)
	{
		Services = app.Services;
		ApplicationBuilder = app;
		EndpointRouteBuilder = app;
		Configuration = app.Configuration;
		Logger = app.Logger;
	}
	public IConfiguration Configuration { get; }
	public IServiceProvider Services { get; }
	public IApplicationBuilder ApplicationBuilder { get; }
	public IEndpointRouteBuilder EndpointRouteBuilder { get; }
	public ILogger Logger { get; }
}