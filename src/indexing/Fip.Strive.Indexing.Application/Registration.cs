using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Features.Indexers;
using Fip.Strive.Indexing.Application.Features.Pagers;
using Fip.Strive.Indexing.Application.Infrastructure.Lite.Contexts;
using Fip.Strive.Indexing.Application.Infrastructure.Lite.Repositories.Contracts;
using Fip.Strive.Indexing.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Indexing.Application;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddIndexingFeature(this IServiceCollection services)
    {
        services.AddSingleton<IndexContext>();

        services.AddScoped<IIndexer<DataIndex, string>, DataIndexer>();
        services.AddScoped<ISpecificationReader<DataIndex>, DataPager>();

        services.AddScoped<IIndexer<FileIndex, string>, FileIndexer>();
        services.AddScoped<ISpecificationReader<FileIndex>, FilePager>();

        services.AddScoped<IIndexer<ZipIndex, string>, ZipIndexer>();
        services.AddScoped<ISpecificationReader<ZipIndex>, ZipPager>();

        services.AddScoped<IInventory, Inventory>();
    }
}
