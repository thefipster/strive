using Fip.Strive.Harvester.Application.Features.Assimilate.Services;
using Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Assimilate.Services.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Assimilate;

public static class Registration
{
    public static void AddAssimilateFeature(this IServiceCollection services)
    {
        services.AddScoped<IAssimilationService, AssimilationService>();

        services.AddScoped<IExtractionService, ExtractionService>();
        services.Decorate<IExtractionService, ExtractionServiceInventorized>();
    }
}
