using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Tracking.Application.Infrastructure.Configurations;

public class TrackerConfiguration : IEntityTypeConfiguration<Tracker>
{
    public void Configure(EntityTypeBuilder<Tracker> builder)
    {
        builder.ToTable("trackers");

        builder.HasKey(tracker => tracker.Id);

        builder.Property(tracker => tracker.Name).IsRequired().HasMaxLength(TrackingLimits.NameLength);

        // Two trackers called "Coffee" would be indistinguishable in every list in the app. The
        // index is case sensitive, which is why TrackerWriter also checks case insensitively.
        builder.HasIndex(tracker => tracker.Name).IsUnique();

        builder.Property(tracker => tracker.Description).HasMaxLength(TrackingLimits.DescriptionLength);

        // Same story as the events' timestamps — see TrackerEventConfiguration.
        builder
            .Property(tracker => tracker.CreatedUtc)
            .HasConversion(TrackingConversions.UtcText)
            .IsRequired();

        builder
            .HasMany(tracker => tracker.Fields)
            .WithOne(field => field.Tracker)
            .HasForeignKey(field => field.TrackerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(tracker => tracker.Events)
            .WithOne(occurrence => occurrence.Tracker)
            .HasForeignKey(occurrence => occurrence.TrackerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
