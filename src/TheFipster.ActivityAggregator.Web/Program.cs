using MudBlazor.Services;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Web;
using TheFipster.ActivityAggregator.Web.Hubs;
using TheFipster.ActivityAggregator.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddSingleton<UploadApi>();
builder.Services.AddSingleton<ScanApi>();
builder.Services.AddSingleton<AssimilateApi>();
builder.Services.AddSingleton<BatchApi>();
builder.Services.AddSingleton<InventoryApi>();

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

app.MapHub<IngestHub>(Const.Hubs.Ingester.Url);

app.Run();
