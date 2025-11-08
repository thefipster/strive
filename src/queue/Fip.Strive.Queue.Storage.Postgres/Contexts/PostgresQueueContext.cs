using Fip.Strive.Queue.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Queue.Storage.Postgres.Contexts;

public class PostgresQueueContext : DbContext
{
    public const string SchemaName = "ingestion-queue";

    public PostgresQueueContext(DbContextOptions<PostgresQueueContext> options)
        : base(options) { }

    public DbSet<JobDetails> Jobs => Set<JobDetails>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgresQueueContext).Assembly);
    }
}
