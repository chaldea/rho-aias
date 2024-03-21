using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.AddRhoAiasServer();

var app = builder.Build();
app.UseAuthorization();
app.UseRhoAiasServer();
app.MapControllers();
app.Run();