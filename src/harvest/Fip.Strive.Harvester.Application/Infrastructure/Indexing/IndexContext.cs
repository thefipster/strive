using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing;

public class IndexContext : DbContext
{
    public const string SchemaName = "harvester-indexer";

    public IndexContext(DbContextOptions<IndexContext> options)
        : base(options) { }

    public DbSet<ZipIndex> Zips => Set<ZipIndex>();
    public DbSet<FileInstance> Files => Set<FileInstance>();
    public DbSet<SourceIndex> Sources => Set<SourceIndex>();
    public DbSet<ExtractIndex> Extracts => Set<ExtractIndex>();
    public DbSet<DataIndex> Data => Set<DataIndex>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IndexContext).Assembly);
    }
}
