using Microsoft.Extensions.DependencyInjection;
using TheFipster.ActivityAggregator.Importer.Features.Classification;
using TheFipster.ActivityAggregator.Importer.Features.Extraction;

namespace TheFipster.ActivityAggregator.Importer;

public static class DependencyExtension
{
    public static void AddImporterFeature(this IServiceCollection services)
    {
        services.AddClassificationFeature();
        services.AddExtractionFeature();
    }
}
