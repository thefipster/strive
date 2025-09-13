using Fipster.TrackMe.Importer.Modules.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Fipster.TrackMe.Importer;

public static class DependencyExtension
{
    public static IServiceCollection AddImporters(this IServiceCollection services)
    {
        services.AddSingleton<IImporterRegistry, Registry>();

        return services;
    }
}
