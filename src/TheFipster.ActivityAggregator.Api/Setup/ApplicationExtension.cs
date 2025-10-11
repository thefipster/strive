using TheFipster.ActivityAggregator.Api.Features.Assimilate.Services;
using TheFipster.ActivityAggregator.Api.Features.Assimilate.Services.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Batch.Components;
using TheFipster.ActivityAggregator.Api.Features.Batch.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Batch.Services;
using TheFipster.ActivityAggregator.Api.Features.Batch.Services.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Core.Components;
using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Core.Services;
using TheFipster.ActivityAggregator.Api.Setup.Application;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite;
using TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

namespace TheFipster.ActivityAggregator.Api.Setup;

public static class ApplicationExtension
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddConfig(configuration);
        services.AddLiteDbStorage(configuration);
        services.AddQueueFeature();
        services.AddUploadFeature();
        services.AddScannerFeature();
        services.AddAssimilateFeature();
        services.AddBatchFeature();

        services.AddScoped<INotifier, Notifier>();
    }

    private static void AddQueueFeature(this IServiceCollection services)
    {
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedHostedService>();
    }

    private static void AddConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiConfig>(configuration.GetSection(ApiConfig.ConfigSectionName));
    }

    private static void AddAssimilateFeature(this IServiceCollection services)
    {
        services.AddScoped<IIndexer<ExtractorIndex>, BaseIndexer<ExtractorIndex>>();
        services.AddScoped<IPagedIndexer<ExtractorIndex>, PagedIndexer<ExtractorIndex>>();

        services.AddScoped<IIndexer<AssimilateIndex>, BaseIndexer<AssimilateIndex>>();
        services.AddScoped<IPagedIndexer<AssimilateIndex>, PagedIndexer<AssimilateIndex>>();

        services.AddScoped<IInventoryIndexer, InventoryIndexer>();

        services.AddScoped<IAssimilaterService, AssimilaterService>();
    }

    private static void AddBatchFeature(this IServiceCollection services)
    {
        services.AddScoped<IIndexer<BatchIndex>, BaseIndexer<BatchIndex>>();
        services.AddScoped<IPagedIndexer<BatchIndex>, PagedIndexer<BatchIndex>>();

        services.AddScoped<IHistoryIndexer, HistoryIndexer>();

        services.AddScoped<IMetricsMerger, MetricsMerger>();
        services.AddScoped<IEventsMerger, EventsMerger>();
        services.AddScoped<ISeriesMerger, SeriesMerger>();

        services.AddScoped<IBatchService, BatchService>();
    }
}
