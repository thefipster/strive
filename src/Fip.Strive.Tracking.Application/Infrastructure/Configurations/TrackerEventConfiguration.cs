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

        // Every event query in the UI is "this tracker, newest first", which this index answers on
        // its own.
        builder.HasIndex(occurrence => new { occurrence.TrackerId, occurrence.OccurredUtc });

        // The export is a different query and the index above cannot serve it: the pull API filters
        // on RecordedUtc and orders by RecordedUtc then Id, so without this it scans and sorts the
        // whole table on every call — and that is the one caller guaranteed to run forever on a
        // timer. Composite rather than a plain RecordedUtc index so it covers the tiebreaker too.
        builder.HasIndex(occurrence => new { occurrence.RecordedUtc, occurrence.Id });

        builder
            .HasMany(occurrence => occurrence.Values)
            .WithOne(value => value.Event)
            .HasForeignKey(value => value.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
