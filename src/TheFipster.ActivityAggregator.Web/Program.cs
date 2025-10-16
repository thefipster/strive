using TheFipster.ActivityAggregator.Web.Components;
using TheFipster.ActivityAggregator.Web.Setup;
using TheFipster.ActivityAggregator.Web.Setup.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddMonitoring(configuration);
builder.Services.AddFrontend();
builder.Services.AddApplicationServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
