using Fip.Strive.Harvester.Application.Features.Expand.Services;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Expand;

public static class Registration
{
    public static void AddExpandFeature(this IServiceCollection services)
    {
        services.AddScoped<IZipExtractor, UnzipCoordinator>();

        services.AddScoped<IZipEnumerator, ZipEnumerator>();

        services.AddScoped<IFileHashGate, FileHashGate>();
        services.Decorate<IFileHashGate, FileHashGateSignalled>();

        services.AddScoped<IExpansionService, ExpandService>();
    }
}
