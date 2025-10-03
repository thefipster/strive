using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Api.Components;
using TheFipster.ActivityAggregator.Api.Services;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Services.Components;
using TheFipster.ActivityAggregator.Services.Worker;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

namespace TheFipster.ActivityAggregator.Api;

public static class Setup
{
    public static IServiceCollection AddCustom(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ApiConfig>(configuration.GetSection(ApiConfig.ConfigSectionName));

        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedHostedService>();

        services.AddTransient<IUploader, Uploader>();

        services.AddTransient<IIndexer<ImporterIndex>, BaseIndexer<ImporterIndex>>();
        services.AddTransient<IExtractor, Extractor>();

        services.AddTransient<IIndexer<ScannerIndex>, BaseIndexer<ScannerIndex>>();
        services.AddSingleton<IImporterRegistry, Registry>();
        services.AddTransient<IScanner, Scanner>();

        services.AddTransient<IIndexer<AssimilaterIndex>, BaseIndexer<AssimilaterIndex>>();
        services.AddTransient<IIndexer<ConvergeIndex>, BaseIndexer<ConvergeIndex>>();
        services.AddTransient<IAssimilater, Assimilater>();
        services.AddTransient<IAssimilaterService, AssimilaterService>();

        services.AddTransient<IInventoryIndexer, InventoryIndexer>();

        services.AddTransient<IUnzipper, Unzipper>();
        services.AddTransient<IIndexer<ZipIndex>, BaseIndexer<ZipIndex>>();
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

        return services;
    }

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowAll",
                policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });

        services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowOne",
                policy =>
                {
                    policy.WithOrigins("https://localhost:7260").AllowAnyMethod().AllowAnyHeader();
                }
            );
        });

        return services;
    }
}
