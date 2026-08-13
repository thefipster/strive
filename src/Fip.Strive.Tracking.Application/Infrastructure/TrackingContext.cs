using Fip.Strive.Tracking.Application.Features.Events.Models;
using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Fip.Strive.Tracking.Application.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Tracking.Application.Infrastructure;

public class TrackingContext(DbContextOptions<TrackingContext> options) : DbContext(options)
{
    public DbSet<Tracker> Trackers => Set<Tracker>();

    public DbSet<TrackerField> TrackerFields => Set<TrackerField>();

    public DbSet<TrackerEvent> TrackerEvents => Set<TrackerEvent>();

    public DbSet<TrackerEventValue> TrackerEventValues => Set<TrackerEventValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TrackerConfiguration());
        modelBuilder.ApplyConfiguration(new TrackerFieldConfiguration());
        modelBuilder.ApplyConfiguration(new TrackerEventConfiguration());
        modelBuilder.ApplyConfiguration(new TrackerEventValueConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
