using Fip.Strive.Harvester.Application.Infrastructure.Indexing;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Fip.Strive.Harvester.Application.Infrastructure.Pipeline.Data;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Harvester.Application.Infrastructure.Pipeline;

public class PipelineContext : DbContext
{
    public const string SchemaName = "harvester-pipeline";

    public PipelineContext(DbContextOptions<PipelineContext> options)
        : base(options) { }

    public DbSet<QuarantinedSignal> Quarantined => Set<QuarantinedSignal>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PipelineContext).Assembly,
            t => t.Namespace != null && t.Namespace.Contains(".Pipeline.")
        );
    }
}
