using Fip.Strive.Harvester.Application.Features.Import.Components;
using Fip.Strive.Harvester.Application.Features.Import.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Services;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Import;

public static class Registration
{
    public static void AddImportFeature(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ImportConfig>(configuration.GetSection(ImportConfig.ConfigSectionName));
        services.AddScoped<IZipIndexer, ZipIndexer>();
        services.AddScoped<IZipInventory, ZipInventory>();
    }
}
