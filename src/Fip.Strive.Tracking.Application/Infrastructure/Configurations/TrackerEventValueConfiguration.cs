using Fip.Strive.Tracking.Application.Features.Events.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Tracking.Application.Infrastructure.Configurations;

public class TrackerEventValueConfiguration : IEntityTypeConfiguration<TrackerEventValue>
{
    public void Configure(EntityTypeBuilder<TrackerEventValue> builder)
    {
        builder.ToTable("tracker_event_values");

        builder.HasKey(value => value.Id);

        builder.Property(value => value.Text).HasMaxLength(TrackingLimits.TextValueLength);

        // One value per field per event, enforced rather than merely intended — the entry form
        // collapses to a single row per field, and duplicates would double every statistic.
        builder.HasIndex(value => new { value.EventId, value.FieldId }).IsUnique();
    }
}
