using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Indexing.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;

public class IndexPgContext : DbContext
{
    public IndexPgContext(DbContextOptions<IndexPgContext> options)
        : base(options) { }

    public DbSet<ZipIndex> Zips => Set<ZipIndex>();
    public DbSet<ZipHashed> ZipsHashed => Set<ZipHashed>();

    public DbSet<FileIndex> Files => Set<FileIndex>();
    public DbSet<FileHashed> FilesHashed => Set<FileHashed>();

    public DbSet<DataIndex> Data => Set<DataIndex>();

    public DbSet<DateEntry> Inventory => Set<DateEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IndexPgContext).Assembly);
    }
}
