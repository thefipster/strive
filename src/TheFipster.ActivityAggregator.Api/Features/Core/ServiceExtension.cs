using TheFipster.ActivityAggregator.Api.Features.Core.Components;
using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Core.Mediators;
using TheFipster.ActivityAggregator.Api.Features.Core.Mediators.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Core.Mediators.Decorators;
using TheFipster.ActivityAggregator.Api.Features.Core.Services;
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
