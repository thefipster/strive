using FluentValidation;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Models.Requests.Validators;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;
using TheFipster.ActivityAggregator.Storage.Lite.Features.Indexing.Components;

namespace TheFipster.ActivityAggregator.Api.Features.Batch;

public static class ServiceExtension
{
    public static void AddBatchFeature(this IServiceCollection services)
    {
        services.AddCommonFeature();

        services.AddBatchingFeature();
        services.AddBatchByDateFeature();
        services.AddBatchPerYearFeature();
        services.AddBatchPageFeature();
        services.AddMergeFileFeature();
    }

    public static void AddCommonFeature(this IServiceCollection services)
    {
        services.AddScoped<IIndexer<BatchIndex>, BaseIndexer<BatchIndex>>();
        services.AddScoped<IPagedIndexer<BatchIndex>, PagedIndexer<BatchIndex>>();
    }

    public static void AddBatchingFeature(this IServiceCollection services)
    {
        services.AddScoped<IBatchAction, BatchAction>();
        services.Decorate<IBatchAction, BatchActionValidator>();

        services.AddScoped<IBatchService, BatchService>();
        services.Decorate<IBatchService, BatchNotifier>();

        services.AddScoped<IAssimilationGrouper, AssimilationGrouper>();
        // AssimilationGroupCombiner alters the return value
        // so it must be registered before AssimilationGrouperWriter
        // which persists the return value to disc and db.
        services.Decorate<IAssimilationGrouper, AssimilationGroupCombiner>();
        services.Decorate<IAssimilationGrouper, AssimilationGrouperWriter>();

        services.AddScoped<IPessimisticMerger, PessimisticMerger>();
        //services.Decorate<IPessimisticMerger, PessimisticMergerIndexer>();

        services.AddScoped<IMetricsMerger, MetricsMerger>();
        services.AddScoped<IEventsMerger, EventsMerger>();
        services.AddScoped<ISeriesNormalizer, SeriesNormalizer>();
    }

    public static void AddBatchByDateFeature(this IServiceCollection services)
    {
        services.AddScoped<IBatchByDateAction, BatchByDateAction>();
        services.Decorate<IBatchByDateAction, BatchByDateValidator>();
    }

    public static void AddBatchPerYearFeature(this IServiceCollection services)
    {
        services.AddScoped<IBatchExistsPerYearAction, BatchExistsPerYearAction>();
        services.Decorate<IBatchExistsPerYearAction, BatchExistsPerYearValidator>();
    }

    public static void AddBatchPageFeature(this IServiceCollection services)
    {
        services.AddScoped<IBatchPageAction, BatchPageAction>();
        services.AddScoped<IValidator<PagedRequest>, PagedRequestValidator>();
        services.Decorate<IBatchPageAction, BatchPageValidator>();
    }

    public static void AddMergeFileFeature(this IServiceCollection services)
    {
        services.AddScoped<IMergeFileAction, MergeFileAction>();
        services.Decorate<IMergeFileAction, MergeFileValidator>();
    }
}
