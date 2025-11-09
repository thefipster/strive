using Fip.Strive.Harvester.Application.Features.Classify.Services;
using Fip.Strive.Harvester.Application.Features.Classify.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Classify;

public static class Registration
{
    public static void AddClassifyFeature(this IServiceCollection services)
    {
        services.AddScoped<IClassifyService, ClassifyService>();
    }
}
