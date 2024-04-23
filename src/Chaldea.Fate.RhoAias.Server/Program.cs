using Serilog;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.AddRhoAiasServer(cfg =>
{
	cfg.AddRhoAiasDashboard();
	cfg.AddRhoAiasSqlite();
	cfg.AddAhoAiasJwtBearerAuthentication();
	cfg.AddRhoAiasPrometheus();
	cfg.AddAhoAiasLetsEncrypt();
	cfg.AddAhoAiasAliyunDns();
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseRhoAiasServer(b =>
{
	b.UseRhoAiasDashboard();
	b.UseRhoAiasSqlite();
	b.UseRhoAiasPrometheus();
	b.UseAhoAiasLetsEncrypt();
});
app.Run();