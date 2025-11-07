using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Application.Services;
using Fip.Strive.Ingestion.Application.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Ingestion.Application;

public static class Registration
{
    public static void AddIngestionFeature(this IServiceCollection services)
    {
        services.Scan(scan =>
            scan.FromAssembliesOf(typeof(IFileClassifier))
                .AddClasses(classes => classes.AssignableTo<IFileClassifier>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.Scan(scan =>
            scan.FromAssembliesOf(typeof(IFileExtractor))
                .AddClasses(classes => classes.AssignableTo<IFileExtractor>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddScoped<IClassifier, Classifier>();
        services.AddScoped<IExtractor, Extractor>();
    }
}
