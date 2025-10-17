using Microsoft.Extensions.DependencyInjection;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Services;
using TheFipster.ActivityAggregator.Storage.Lite.Features.Indexing.Context;
using TheFipster.ActivityAggregator.Storage.Lite.Features.Indexing.Services;

namespace TheFipster.ActivityAggregator.Storage.Lite.Features.Indexing;

public static class DependencyExtension
{
    public static void AddIndexingFeature(this IServiceCollection services)
    {
        services.AddSingleton<IndexerContext>();

        services.AddScoped<IHistoryIndexer, HistoryIndexer>();
        services.AddScoped<IInventoryIndexer, InventoryIndexer>();
    }
}
