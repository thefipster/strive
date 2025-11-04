using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application.Infrastructure.Contracts;
using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Features.Indexers;
using Fip.Strive.Indexing.Application.Features.Pagers;
using Fip.Strive.Indexing.Application.Infrastructure.Lite.Contexts;
using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;
using Fip.Strive.Indexing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Indexing.Application;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddPostgresInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<IndexPgContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("strive-harvester-index"))
        );

        services.AddScoped<IIndexer<ZipIndex, string>, PgZipIndexer>();
        services.AddScoped<ISpecificationReader<ZipIndex>, PgZipPager>();

        services.AddScoped<IIndexer<FileIndex, string>, PgFileIndexer>();
        services.AddScoped<ISpecificationReader<FileIndex>, PgFilePager>();

        services.AddScoped<IIndexer<DataIndex, string>, PgDataIndexer>();
        services.AddScoped<ISpecificationReader<DataIndex>, PgDataPager>();

        services.AddScoped<IInventory, PgInventory>();
    }

    public static void AddLiteDbInfrastructure(this IServiceCollection services)
    {
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
