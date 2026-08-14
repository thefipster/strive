using Fip.Strive.Application.Features.Jobs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Application.Infrastructure.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs");

        builder.HasKey(job => job.Id);

        builder.Property(job => job.Kind).HasMaxLength(64).IsRequired();

        builder.Property(job => job.TargetKey).HasMaxLength(512).IsRequired();

        builder.Property(job => job.ComponentId).HasMaxLength(128).IsRequired();

        // Stored as the enum's name rather than its ordinal: the column stays readable in psql,
        // and inserting a member in the middle of the enum later cannot silently reinterpret every
        // existing row.
        builder.Property(job => job.State).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(job => job.Payload).HasColumnType("jsonb");

        builder.Property(job => job.ProgressNote).HasMaxLength(1024);

        // A work unit exists once. Enqueueing a known unit is an upsert back to Pending, which is
        // what the spec means by a unit recording the version that last succeeded.
        builder.HasIndex(job => new { job.Kind, job.TargetKey }).IsUnique();

        // The pump's claim query.
        builder.HasIndex(job => new { job.State, job.EnqueuedUtc });

        // Step 3's invalidation sweep.
        builder.HasIndex(job => new { job.ComponentId, job.ComponentVersion });
    }
}
