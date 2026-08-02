using Microsoft.Extensions.DependencyInjection;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Contracts;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Services;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Services.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification;

public static class DependencyExtension
{
    public static void AddClassificationFeature(this IServiceCollection services)
    {
        services.AddImplementations<IFileClassifier>();
        services.AddScoped<IClassifier, Classifier>();
    }
}
