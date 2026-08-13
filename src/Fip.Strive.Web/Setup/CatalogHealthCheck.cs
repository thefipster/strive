using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fip.Strive.Web.Setup;

/// <summary>
/// Asks Postgres a question rather than merely opening a socket to it. An <c>EXISTS</c> against
/// the catalog proves the connection, the credentials, the database and the migrated schema in one
/// round trip, and Postgres stops at the first row — so it stays cheap no matter how large the
/// catalog grows.
/// </summary>
public sealed class CatalogHealthCheck(StriveContext database) : IHealthCheck
{
    public const string Name = "postgres";

    /// <summary>Bounds a probe against an unreachable host, which would otherwise sit on the connection timeout.</summary>
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await database.CatalogEntries.AsNoTracking().AnyAsync(cancellationToken);
            return HealthCheckResult.Healthy("The catalog database answered.");
        }
        catch (Exception exception)
        {
            // The endpoint is public, so the description says only that something is wrong. The
            // exception travels to the log, where the host and the connection string belong.
            return HealthCheckResult.Unhealthy("The catalog database did not answer.", exception);
        }
    }
}
