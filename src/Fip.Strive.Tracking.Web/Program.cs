using System.Text.Json.Serialization;
using Fip.Strive.Tracking.Application;
using Fip.Strive.Tracking.Web.Access;
using Fip.Strive.Tracking.Web.Api;
using Fip.Strive.Tracking.Web.Components;
using Fip.Strive.Tracking.Web.Setup;

// Configuration holds a hash, never the password itself, so there has to be something that turns
// one into the other. Runs before the host is built — the host refuses to start without the hash
// this command exists to produce.
if (args is ["hash-password", ..])
{
    Environment.ExitCode = PasswordCommand.Run(args);
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMonitoring(builder.Configuration);
builder.Services.AddFrontend();
builder.Services.AddHealthEndpoint();
builder.Services.AddAccess(builder.Configuration, builder.Environment);
builder.Services.AddTracking(builder.Configuration, builder.Environment.ContentRootPath);

// Field types travel as their names rather than as 1 and 2 — the API is read by another codebase,
// and a renumbered enum should break loudly there instead of silently mismapping.
builder.Services.ConfigureHttpJsonOptions(json =>
    json.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
);

var app = builder.Build();

await app.EnsureDatabaseReadyAsync();

app.UseRequestLogging();
app.UseErrorHandling();
app.UseHttps();
app.MapStaticAssets();
app.UseAccess();
app.UseAntiforgery();
app.MapAccess();
app.MapTrackingApi();
app.UseFrontend<App>();
app.UseHealthEndpoint();

await app.RunAsync();
