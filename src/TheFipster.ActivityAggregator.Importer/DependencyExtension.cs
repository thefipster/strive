using Microsoft.Extensions.DependencyInjection;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer;

public static class DependencyExtension
{
    public static IServiceCollection AddImporters(this IServiceCollection services)
    {
        services.AddSingleton<IImporterRegistry, Registry>();

        return services;
    }
}
