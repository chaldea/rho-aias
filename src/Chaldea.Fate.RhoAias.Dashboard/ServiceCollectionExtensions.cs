using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddRhoAiasDashboar(this IServiceCollection services)
	{
		services.AddControllers();
		services.AddSpaStaticFiles(options =>
		{
			options.RootPath = "wwwroot/dashboard";
		});
		return services;
	}

	public static WebApplication UseRhoAiasDashboard(this WebApplication app)
	{
		app.UseSpaStaticFiles();
		app.UseSpa(spa =>
		{
			spa.Options.DefaultPage = "/index.html";
		});
		app.MapControllers();
		return app;
	}
}