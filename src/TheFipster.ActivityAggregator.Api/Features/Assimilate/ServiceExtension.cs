using FluentValidation;
using TheFipster.ActivityAggregator.Api.Extensions;
using TheFipster.ActivityAggregator.Api.Interceptors;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate;

public static class ServiceExtension
{
    public static void AddAssimilateFeature(this IServiceCollection services)
    {
        services.AddExtractorInfoFeature();
        services.AddAssimilationFeature();
        services.AddExtractsPageFeature();
    }

    private static void AddExtractorInfoFeature(this IServiceCollection services)
    {
        services.AddScoped<IExtractorsAction, ExtractorsAction>();
    }

    private static void AddAssimilationFeature(this IServiceCollection services)
    {
        services.AddScoped<IAssimilateAction, AssimilateAction>();

        services.AddInterceptedScoped<IAssimilaterService, AssimilaterService>(
            typeof(TracingInterceptor)
        );
        services.Decorate<IAssimilaterService, AssimilaterNotifier>();

        services.AddInterceptedScoped<IFileAssimilator, FileAssimilator>(
            typeof(TracingInterceptor)
        );
        services.Decorate<IFileAssimilator, FileAssimilatorIndexer>();

        services.AddScoped<IIndexer<ExtractorIndex>, BaseIndexer<ExtractorIndex>>();

        services.AddScoped<IExtractionCataloger, ExtractionCataloger>();
        services.Decorate<IExtractionCataloger, ExtractionCatalogerIndexer>();
        services.Decorate<IExtractionCataloger, ExtractionCatalogerInventory>();

        services.AddScoped<IIndexer<AssimilateIndex>, BaseIndexer<AssimilateIndex>>();
    }

    private static void AddExtractsPageFeature(this IServiceCollection services)
    {
        services.AddScoped<IExtractPageAction, ExtractPageAction>();
        services.AddScoped<
            IValidator<AssimilateExtractPageRequest>,
            AssimilateExtractPageRequestValidator
        >();
        services.Decorate<IExtractPageAction, ExtractPageValidation>();

        services.AddScoped<IPagedIndexer<ExtractorIndex>, PagedIndexer<ExtractorIndex>>();
        services.AddScoped<IPagedIndexer<AssimilateIndex>, PagedIndexer<AssimilateIndex>>();
    }
}
