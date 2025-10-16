using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Setup;

public static class ApplicationExtension
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<UploadApi>();
        services.AddSingleton<ScanApi>();
        services.AddSingleton<AssimilateApi>();
        services.AddSingleton<BatchApi>();
        services.AddSingleton<InventoryApi>();
        services.AddSingleton<HistoryApi>();
    }
}
