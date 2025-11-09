using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application.Infrastructure.Contracts;
using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;
using Fip.Strive.Indexing.Storage.Postgres.Contexts;
using Fip.Strive.Indexing.Storage.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Indexing.Storage.Postgres;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void WithPostgresStorage(this IndexingFeatureBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddDbContext<IndexPgContext>(
            options => options.UseNpgsql(configuration.GetConnectionString("strive-harvester")),
            ServiceLifetime.Transient
        );

        services.AddTransient<IIndexer<ZipIndex, string>, PgZipIndexer>();
        services.AddTransient<ISpecificationReader<ZipIndex>, PgZipPager>();

        services.AddTransient<IIndexer<FileIndex, string>, PgFileIndexer>();
        services.AddTransient<ISpecificationReader<FileIndex>, PgFilePager>();

        services.AddTransient<IIndexer<DataIndex, string>, PgDataIndexer>();
        services.AddTransient<ISpecificationReader<DataIndex>, PgDataPager>();

        services.AddTransient<IInventory, PgInventory>();
    }
}
