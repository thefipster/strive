using Microsoft.Extensions.DependencyInjection;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Services;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Services.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction;

public static class DependencyExtension
{
    public static void AddExtractionFeature(this IServiceCollection services)
    {
        services.AddImplementations<IFileExtractor>();
        services.AddScoped<IExtractor, Extractor>();
    }
}
