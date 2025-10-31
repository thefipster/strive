using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Core.Indexing.Repositories;
using Fip.Strive.Harvester.Application.Core.Indexing.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Components;
using Fip.Strive.Harvester.Application.Features.Import.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Components.Decorators;
using Fip.Strive.Harvester.Application.Features.Import.Repositories;
using Fip.Strive.Harvester.Application.Features.Import.Services;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Services.Decorators;
using Fip.Strive.Harvester.Application.Infrastructure.Repositories.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Import;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddImportFeature(this IServiceCollection services)
    {
        services.AddScoped<IIndexer<ZipIndex, string>, ZipIndexer>();
        services.AddScoped<ISpecificationReader<ZipIndex>, ZipPager>();

        services.AddScoped<IZipFileAccess, ZipFileAccess>();
        services.Decorate<IZipFileAccess, ZipFileAccessValidator>();

        services.AddScoped<IZipInventory, ZipInventory>();
        services.Decorate<IZipInventory, ZipInventorySignaller>();
    }
}
