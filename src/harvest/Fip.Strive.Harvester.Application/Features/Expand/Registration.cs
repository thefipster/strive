using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Expand;

public static class Registration
{
    public static void AddExpandFeature(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ExpandConfig>(configuration.GetSection(ExpandConfig.ConfigSectionName));
    }
}
