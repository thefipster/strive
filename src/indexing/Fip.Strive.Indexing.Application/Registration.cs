using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Indexing.Application.Contexts;
using Fip.Strive.Indexing.Application.Repositories;
using Fip.Strive.Indexing.Application.Repositories.Contracts;
using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Indexing.Application;

internal sealed class IndexingFeature;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static IndexingFeatureBuilder AddIndexingFeature(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSingleton<AssimilateContext>();
        services.AddScoped<IAssimilateRepository, AssimilateRepository>();

        services.AddSingleton<FileContext>();
        services.AddScoped<IFileRepository, FileRepository>();

        services.AddSingleton<TypeContext>();
        services.AddScoped<ITypeRepository, TypeRepository>();

        services.AddSingleton<ExtractContext>();
        services.AddScoped<IIndexerV2<ExtractIndexV2, string>, ExtractRepository>();

        services.AddSingleton<ZipContext>();
        services.AddScoped<IIndexerV2<ZipIndexV2, string>, ZipRepository>();

        services.AddSingleton<InventoryContext>();
        services.AddScoped<IInventory, Inventory>();

        return new IndexingFeatureBuilder(services, configuration);
    }
}
