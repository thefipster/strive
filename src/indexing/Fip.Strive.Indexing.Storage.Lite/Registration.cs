using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application.Features.Config;
using Fip.Strive.Core.Application.Infrastructure.Contracts;
using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;
using Fip.Strive.Indexing.Storage.Lite.Contexts;
using Fip.Strive.Indexing.Storage.Lite.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Indexing.Storage.Lite;

internal sealed class IndexingLiteStorage;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void WithLiteStorage(this IndexingFeatureBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddConfigs<IndexingLiteStorage>(configuration);

        services.AddSingleton<IndexLiteContext>();

        services.AddScoped<IIndexer<ZipIndex, string>, LiteZipIndexer>();
        services.AddScoped<ISpecificationReader<ZipIndex>, LiteZipPager>();

        services.AddScoped<IIndexer<FileIndex, string>, LiteFileIndexer>();
        services.AddScoped<ISpecificationReader<FileIndex>, LiteFilePager>();

        services.AddScoped<IIndexer<DataIndex, string>, LiteDataIndexer>();
        services.AddScoped<ISpecificationReader<DataIndex>, LiteDataPager>();

        services.AddScoped<IInventory, LiteInventory>();
    }
}
