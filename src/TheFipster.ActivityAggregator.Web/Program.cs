using MudBlazor.Services;
using TheFipster.ActivityAggregator.Web;
using TheFipster.ActivityAggregator.Web.Hubs;
using TheFipster.ActivityAggregator.Web.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddSingleton<ApiService>();

builder.Services.AddSignalR(e =>
{
    e.EnableDetailedErrors = true;
    e.MaximumReceiveMessageSize = 102400000;
});

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

app.MapHub<EventHub>("/eventhub");
app.MapHub<ScanHub>("/scanhub");
app.MapHub<AssimilateHub>("/hubs/assimilate");

app.Run();
