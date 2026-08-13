using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Tracking.Application.Infrastructure.Configurations;

public class TrackerFieldConfiguration : IEntityTypeConfiguration<TrackerField>
{
    public void Configure(EntityTypeBuilder<TrackerField> builder)
    {
        builder.ToTable("tracker_fields");

        builder.HasKey(field => field.Id);

        builder.Property(field => field.Name).IsRequired().HasMaxLength(TrackingLimits.NameLength);

        builder.Property(field => field.Unit).HasMaxLength(TrackingLimits.UnitLength);

        // Stored as the enum's number so renaming a member cannot silently retype every value.
        builder.Property(field => field.Type).HasConversion<int>().IsRequired();

        builder.Property(field => field.SortOrder).IsRequired();

        builder.HasIndex(field => new { field.TrackerId, field.Name }).IsUnique();

        builder
            .HasMany(field => field.Values)
            .WithOne(value => value.Field)
            .HasForeignKey(value => value.FieldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
