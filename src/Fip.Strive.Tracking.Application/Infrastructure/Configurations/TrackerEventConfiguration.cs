using Fip.Strive.Tracking.Application.Features.Events.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Tracking.Application.Infrastructure.Configurations;

public class TrackerEventConfiguration : IEntityTypeConfiguration<TrackerEvent>
{
    public void Configure(EntityTypeBuilder<TrackerEvent> builder)
    {
        builder.ToTable("tracker_events");

        builder.HasKey(occurrence => occurrence.Id);

        // Left to the provider's own mapping, SQLite refuses to ORDER BY or MAX a DateTimeOffset
        // at all — and "newest first" is the only order this table is ever read in. Converted to a
        // fixed-width ISO string it sorts correctly, because every row is stored as UTC, and it
        // stays readable to anyone opening the file in a SQLite browser.
        builder
            .Property(occurrence => occurrence.OccurredUtc)
            .HasConversion(TrackingConversions.UtcText)
            .IsRequired();

        builder
            .Property(occurrence => occurrence.RecordedUtc)
            .HasConversion(TrackingConversions.UtcText)
            .IsRequired();

        builder.Property(occurrence => occurrence.Note).HasMaxLength(TrackingLimits.NoteLength);

        // Every event query is "this tracker, newest first", which this index answers on its own.
        builder.HasIndex(occurrence => new { occurrence.TrackerId, occurrence.OccurredUtc });

        builder
            .HasMany(occurrence => occurrence.Values)
            .WithOne(value => value.Event)
            .HasForeignKey(value => value.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
