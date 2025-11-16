using Fip.Strive.Indexing.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Indexing.Storage.Postgres.Contexts;

public class IndexPgContext : DbContext
{
    public const string SchemaName = "ingestion-index";

    public IndexPgContext(DbContextOptions<IndexPgContext> options)
        : base(options) { }

    public DbSet<ZipIndex> Zips => Set<ZipIndex>();
    public DbSet<ZipHashed> ZipsHashed => Set<ZipHashed>();

    public DbSet<FileIndex> Files => Set<FileIndex>();
    public DbSet<FileHashed> FilesHashed => Set<FileHashed>();

    public DbSet<DataIndex> Data => Set<DataIndex>();

    public DbSet<DateEntry> Inventory => Set<DateEntry>();

    public DbSet<ZipIndexV2> ZipsV2 => Set<ZipIndexV2>();
    public DbSet<FileIndexV2> FilesV2 => Set<FileIndexV2>();
    public DbSet<TypeIndexV2> TypesV2 => Set<TypeIndexV2>();
    public DbSet<AssimilateIndexV2> AssimilatesV2 => Set<AssimilateIndexV2>();
    public DbSet<ExtractIndexV2> ExtractsV2 => Set<ExtractIndexV2>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IndexPgContext).Assembly);
    }
}
