using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = Host.CreateDefaultBuilder(args);
builder.UseSerilog((context, services, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddRhoAiasClient(hostContext.Configuration);
    });
var host = builder.Build();
host.Run();
