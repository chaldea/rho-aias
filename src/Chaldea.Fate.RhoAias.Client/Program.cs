using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Information()
    .CreateLogger();

var builder = Host.CreateDefaultBuilder();
builder.UseSerilog()
    .ConfigureHostConfiguration(config =>
    {
        // appsettings.json have a higher priority and only work when appsettings.json don't exist.
        config.AddCommandLine(args, new Dictionary<string, string>
        {
            { "--server", "RhoAias:Client:ServerUrl" },
            { "-s", "RhoAias:Client:ServerUrl" },
            { "--token", "RhoAias:Client:Token" },
            { "-t", "RhoAias:Client:Token" }
        });
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddRhoAiasClient(hostContext.Configuration);
        services.AddRhoAiasSnappyCompression();
#if INGRESS
		services.AddAhoAiasIngressController(hostContext.Configuration);
#endif
    });
var host = builder.Build();
host.Run();