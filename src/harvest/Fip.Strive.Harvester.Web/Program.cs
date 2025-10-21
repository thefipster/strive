using Fip.Strive.Harvester.Application;
using Fip.Strive.Harvester.Web.Setup.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddMonitoring(configuration);
builder.Services.AddFrontend();
builder.Services.AddApplicationServices(configuration);

var app = builder.Build();

app.UseErrorHandling();
app.UseHttps();
app.MapStaticAssets();
app.UseAntiforgery();
app.UseFrontend();
app.UseMonitoring();
app.Run();
