using Fip.Strive.Harvester.Web.Setup;
using Fip.Strive.Harvester.Web.Setup.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddCompression(configuration);
builder.Services.AddMonitoring(configuration);
builder.Services.AddFrontend();
builder.Services.AddApplication(configuration);

var app = builder.Build();

app.UseCompression();
app.UseErrorHandling();
app.UseHttps();
app.MapStaticAssets();
app.UseAntiforgery();
app.UseFrontend();
app.UseMonitoring();
app.UseApplication();
app.Run();
