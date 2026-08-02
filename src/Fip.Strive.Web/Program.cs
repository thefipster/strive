using Fip.Strive.Application;
using Fip.Strive.Web.Components;
using Fip.Strive.Web.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMonitoring(builder.Configuration);
builder.Services.AddFrontend();
builder.Services.AddHealthEndpoint();
builder.Services.AddApplication(builder.Configuration, builder.Environment.ContentRootPath);

var app = builder.Build();

app.EnsureStorageReady();
await app.MigrateDatabaseAsync();

app.UseRequestLogging();
app.UseErrorHandling();
app.UseHttps();
app.MapStaticAssets();
app.UseAntiforgery();
app.UseFrontend<App>();
app.UseHealthEndpoint();

await app.RunAsync();

// Exposed so the test host can reference this assembly via WebApplicationFactory<Program>.
public partial class Program;
