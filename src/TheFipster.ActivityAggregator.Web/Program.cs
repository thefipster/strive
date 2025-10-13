using MudBlazor.Services;
using Serilog;
using SerilogTracing;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Web;
using TheFipster.ActivityAggregator.Web.Hubs;
using TheFipster.ActivityAggregator.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(c => c.ReadFrom.Configuration(builder.Configuration));
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddSingleton<UploadApi>();
builder.Services.AddSingleton<ScanApi>();
builder.Services.AddSingleton<AssimilateApi>();
builder.Services.AddSingleton<BatchApi>();
builder.Services.AddSingleton<InventoryApi>();
builder.Services.AddSingleton<HistoryApi>();

using var listener = new ActivityListenerConfiguration()
    .Instrument.WithDefaultInstrumentation(false)
    .Instrument.HttpClientRequests(opts =>
        opts.MessageTemplate = nameof(TheFipster.ActivityAggregator.Web)
    )
    .TraceToSharedLogger();

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
app.MapHub<IngestHub>(Defaults.Hubs.Ingester.Url);
app.Run();
