using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Core.Indexing.Indexers;
using Fip.Strive.Harvester.Application.Core.Indexing.Pagers;
using Fip.Strive.Harvester.Application.Infrastructure.Repositories.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Core.Indexing;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddIndexingFeature(this IServiceCollection services)
    {
        services.AddScoped<IIndexer<DataIndex, string>, DataIndexer>();
        services.AddScoped<ISpecificationReader<DataIndex>, DataPager>();

        services.AddScoped<IIndexer<FileIndex, string>, FileIndexer>();
        services.AddScoped<ISpecificationReader<FileIndex>, FilePager>();

        services.AddScoped<IIndexer<ZipIndex, string>, ZipIndexer>();
        services.AddScoped<ISpecificationReader<ZipIndex>, ZipPager>();

        services.AddScoped<IInventory, Inventory>();
    }
}
