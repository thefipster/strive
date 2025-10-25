using Fip.Strive.Harvester.Application.Features.Expand.Component;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Decorators;
using Fip.Strive.Harvester.Application.Features.Expand.Repositories;
using Fip.Strive.Harvester.Application.Features.Expand.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Services;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;
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

        services.AddScoped<IUnzipper, Unzipper>();

        services.AddScoped<IScanner, Scanner>();

        services.AddScoped<IChecker, Checker>();
        services.Decorate<IChecker, CheckerSignalled>();

        services.AddScoped<IFileIndexer, FileIndexer>();

        services.AddScoped<IExpansionService, ExpansionService>();
    }
}
