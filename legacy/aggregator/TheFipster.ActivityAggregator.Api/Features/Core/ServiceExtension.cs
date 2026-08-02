using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Services;
using TheFipster.ActivityAggregator.Storage.Lite.Features.Indexing.Services;

namespace TheFipster.ActivityAggregator.Api.Features.Core;

public static class ServiceExtension
{
    public static void AddCoreFeature(this IServiceCollection services)
    {
        services.AddScoped<INotifier, Notifier>();

        services.AddScoped<IHistoryIndexer, HistoryIndexer>();
        services.AddScoped<IInventoryIndexer, InventoryIndexer>();

        services.AddScoped<IImportHistoryAction, ImportHistoryAction>();
        services.Decorate<IImportHistoryAction, ImportHistoryValidator>();
    }
}
