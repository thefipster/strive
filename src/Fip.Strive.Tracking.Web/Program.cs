using Fip.Strive.Tracking.Application;
using Fip.Strive.Tracking.Web.Components;
using Fip.Strive.Tracking.Web.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMonitoring(builder.Configuration);
builder.Services.AddFrontend();
builder.Services.AddHealthEndpoint();
builder.Services.AddTracking(builder.Configuration, builder.Environment.ContentRootPath);

var app = builder.Build();

await app.EnsureDatabaseReadyAsync();

app.UseRequestLogging();
app.UseErrorHandling();
app.UseHttps();
app.MapStaticAssets();
app.UseAntiforgery();
app.UseFrontend<App>();
app.UseHealthEndpoint();

await app.RunAsync();
