using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Core;

public static class ServiceExtension
{
    public static void AddCoreFeature(this IServiceCollection services)
    {
        services.AddHostedService<QueuedHostedService>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

        services.AddScoped<INotifier, Notifier>();

        services.AddScoped<IHistoryIndexer, HistoryIndexer>();
        services.AddScoped<IInventoryIndexer, InventoryIndexer>();

        services.AddScoped<IImportHistoryAction, ImportHistoryAction>();
        services.Decorate<IImportHistoryAction, ImportHistoryValidator>();
    }
}
