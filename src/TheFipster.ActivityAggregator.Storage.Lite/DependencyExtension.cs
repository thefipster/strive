using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Activity;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Components.Activity;
using TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite;

public static class DependencyExtension
{
    public static void AddLiteDbStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<LiteDbConfig>(configuration.GetSection(LiteDbConfig.ConfigSectionName));

        services.AddSingleton<IndexerContext>();
        services.AddScoped<IIndexer<ImportIndex>, ImportIndexer>();
        services.AddScoped<IIndexer<ScanIndex>, ScanIndexer>();
        services.AddScoped<IIndexer<ClassificationIndex>, ClassificationIndexer>();
        services.AddScoped<IIndexer<TransformIndex>, TransformIndexer>();
        services.AddScoped<IIndexer<BundleIndex>, BundleIndexer>();
        services.AddScoped<IIndexer<UnifyIndex>, UnifiyIndexer>();
        services.AddScoped<IMasterIndexer, MasterIndexer>();

        services.AddSingleton<ActivityContext>();
        services.AddScoped<IUnifiedRecordWriter, UnifiedRecordWriter>();
    }
}
