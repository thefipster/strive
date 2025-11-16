using Fip.Strive.Harvester.Domain.Indexes;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Harvester.Application.Infrastructure.Contexts;

public class IndexContext : DbContext
{
    public const string SchemaName = "harvester-indexer";

    public IndexContext(DbContextOptions<IndexContext> options)
        : base(options) { }

    public DbSet<ZipIndex> Zips => Set<ZipIndex>();
    public DbSet<FileInstance> Files => Set<FileInstance>();
    public DbSet<SourceIndex> Sources => Set<SourceIndex>();

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
