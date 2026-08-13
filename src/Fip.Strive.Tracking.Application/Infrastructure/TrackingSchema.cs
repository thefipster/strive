using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Tracking.Application.Infrastructure;

public static class TrackingSchema
{
    /// <summary>
    /// Creates the tables on first run and does nothing on every run after that. There are no EF
    /// migrations here on purpose: the whole database is one file belonging to one person, so a
    /// schema change is handled by moving the old file aside rather than by a migration chain. Add
    /// migrations the day that stops being true.
    /// </summary>
    public static async Task EnsureCreatedAsync(
        this TrackingContext context,
        CancellationToken cancellationToken = default
    ) => await context.Database.EnsureCreatedAsync(cancellationToken);
}
