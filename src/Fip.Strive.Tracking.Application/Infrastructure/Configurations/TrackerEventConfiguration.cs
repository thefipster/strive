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

        builder.Property(occurrence => occurrence.OccurredUtc).IsRequired();

        builder.Property(occurrence => occurrence.RecordedUtc).IsRequired();

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
