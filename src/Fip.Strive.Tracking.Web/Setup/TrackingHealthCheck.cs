using Fip.Strive.Tracking.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fip.Strive.Tracking.Web.Setup;

/// <summary>
/// Queries the SQLite file rather than checking that it exists. Opening a connection succeeds
/// against an empty or half-written file, so the check reads from a table instead: that covers the
/// file being present, readable, and actually carrying the schema.
/// </summary>
/// <remarks>
/// This is the one check that would catch a stale database after a model change. The app creates
/// its schema with <c>EnsureCreated</c> and has no migrations, so an existing file is left exactly
/// as it was — a new table or column simply will not be there, and the failure would otherwise
/// wait for whichever page happens to touch it first.
/// </remarks>
public sealed class TrackingHealthCheck(TrackingContext database) : IHealthCheck
{
    public const string Name = "sqlite";

    /// <summary>A local file should answer immediately; anything slower means a locked or wedged database.</summary>
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await database.Trackers.AsNoTracking().AnyAsync(cancellationToken);
            return HealthCheckResult.Healthy("The tracking database answered.");
        }
        catch (Exception exception)
        {
            // The description stays vague on purpose — it is the file path and the SQLite error
            // that are worth having, and those go to the log rather than over the wire.
            return HealthCheckResult.Unhealthy("The tracking database did not answer.", exception);
        }
    }
}
