using TheFipster.ActivityAggregator.Api.Features.Assimilate;
using TheFipster.ActivityAggregator.Api.Features.Batch.Components;
using TheFipster.ActivityAggregator.Api.Features.Batch.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Batch.Services;
using TheFipster.ActivityAggregator.Api.Features.Batch.Services.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Core;
using TheFipster.ActivityAggregator.Api.Features.Scan;
using TheFipster.ActivityAggregator.Api.Features.Upload;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer;
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
        services.AddImporters();
        services.AddLiteDbStorage(configuration);
        services.AddCoreFeature();
        services.AddUploadFeature();
        services.AddScannerFeature();
        services.AddAssimilateFeature();
        services.AddBatchFeature();
    }

    private static void AddConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiConfig>(configuration.GetSection(ApiConfig.ConfigSectionName));
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
