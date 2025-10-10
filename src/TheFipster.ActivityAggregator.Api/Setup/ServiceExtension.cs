using TheFipster.ActivityAggregator.Api.Components;
using TheFipster.ActivityAggregator.Api.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Components.Decoration;
using TheFipster.ActivityAggregator.Api.Extensions;
using TheFipster.ActivityAggregator.Api.Interceptors;
using TheFipster.ActivityAggregator.Api.Mediators.Upload;
using TheFipster.ActivityAggregator.Api.Mediators.Upload.Contracts;
using TheFipster.ActivityAggregator.Api.Mediators.Upload.Decorators;
using TheFipster.ActivityAggregator.Api.Services;
using TheFipster.ActivityAggregator.Api.Services.Contracts;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

namespace TheFipster.ActivityAggregator.Api.Setup;

public static class ServiceExtension
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ApiConfig>(configuration.GetSection(ApiConfig.ConfigSectionName));

        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedHostedService>();

        services.AddTransient<IUploader, Uploader>();

        services.AddSingleton<IImporterRegistry, Registry>();

        services.AddTransient<IAssimilaterService, AssimilaterService>();

        services.AddTransient<IInventoryIndexer, InventoryIndexer>();

        services.AddTransient<IUnzipper, Unzipper>();
        services.Decorate<IUnzipper, UnzipperValidator>();

        services.AddTransient<IIndexer<ZipIndex>, BaseIndexer<ZipIndex>>();
        services.AddTransient<IPagedIndexer<ZipIndex>, PagedIndexer<ZipIndex>>();
        services.AddTransient<IUnzipService, UnzipService>();

        services.AddTransient<IClassifier, Classifier>();
        services.AddTransient<IIndexer<FileIndex>, BaseIndexer<FileIndex>>();
        services.AddTransient<IPagedIndexer<FileIndex>, PagedIndexer<FileIndex>>();
        services.AddTransient<IScannerService, ScannerService>();

        services.AddTransient<IIndexer<ExtractorIndex>, BaseIndexer<ExtractorIndex>>();
        services.AddTransient<IPagedIndexer<ExtractorIndex>, PagedIndexer<ExtractorIndex>>();

        services.AddTransient<IIndexer<BatchIndex>, BaseIndexer<BatchIndex>>();
        services.AddTransient<IPagedIndexer<BatchIndex>, PagedIndexer<BatchIndex>>();
        services.AddTransient<IMetricsMerger, MetricsMerger>();
        services.AddTransient<IEventsMerger, EventsMerger>();
        services.AddTransient<ISeriesMerger, SeriesMerger>();
        services.AddTransient<IBatchService, BatchService>();

        services.AddInterceptedTransient<IChunkAction, ChunkAction>(typeof(VerboseInterceptor));
        services.Decorate<IChunkAction, ChunkActionValidator>();

        services.AddInterceptedTransient<IZipsAction, ZipsAction>(
            typeof(VerboseInterceptor),
            typeof(Tracinginterceptor)
        );
        services.Decorate<IZipsAction, ZipsActionValidator>();

        services.AddTransient<IHistoryIndexer, HistoryIndexer>();

        services.AddTransient<IIndexer<AssimilateIndex>, BaseIndexer<AssimilateIndex>>();
        services.AddTransient<IPagedIndexer<AssimilateIndex>, PagedIndexer<AssimilateIndex>>();
    }
}
