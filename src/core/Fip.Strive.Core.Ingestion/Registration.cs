using Fip.Strive.Core.Ingestion.Contracts;
using Fip.Strive.Core.Ingestion.Services;
using Fip.Strive.Core.Ingestion.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Core.Ingestion;

public static class Registration
{
    public static void AddIngestionFeature(this IServiceCollection services)
    {
        services.Scan(scan =>
            scan.FromAssembliesOf(typeof(IFileClassifier))
                .AddClasses(classes => classes.AssignableTo<IFileClassifier>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
        );

        services.Scan(scan =>
            scan.FromAssembliesOf(typeof(IFileExtractor))
                .AddClasses(classes => classes.AssignableTo<IFileExtractor>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
        );

        services.AddSingleton<IClassifier, Classifier>();
        services.AddSingleton<IExtractor, Extractor>();
    }
}
