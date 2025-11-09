using Fip.Strive.Harvester.Application.Features.Expand.Services;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Expand;

public static class Registration
{
    public static void AddExpandFeature(this IServiceCollection services)
    {
        services.AddScoped<IExpandService, ExpandService>();

        services.AddScoped<IFileEmitter, FileEmitter>();
    }
}
