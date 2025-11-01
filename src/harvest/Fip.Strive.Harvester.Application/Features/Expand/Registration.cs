using Fip.Strive.Harvester.Application.Features.Expand.Component;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Decorators;
using Fip.Strive.Harvester.Application.Features.Expand.Services;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;
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

        services.AddScoped<IExpansionService, ExpansionService>();
    }
}
