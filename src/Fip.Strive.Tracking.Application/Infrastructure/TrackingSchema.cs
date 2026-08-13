using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Tracking.Application.Infrastructure;

public static class TrackingSchema
{
    /// <summary>
    /// Bumped by hand whenever the model changes in a way an existing file cannot satisfy. There is
    /// no migration to go with it — the remedy is still to move the old file aside — so this exists
    /// only to make that moment legible.
    /// </summary>
    public const int SchemaVersion = 1;

    /// <summary>
    /// Creates the tables on first run and does nothing on every run after that. There are no EF
    /// migrations here on purpose: the whole database is one file belonging to one person, so a
    /// schema change is handled by moving the old file aside rather than by a migration chain. Add
    /// migrations the day that stops being true.
    /// </summary>
    /// <remarks>
    /// <c>EnsureCreated</c> alone is silent about the one case that hurts. It sees a file, decides
    /// there is nothing to do, and the app starts perfectly happily — then fails at the first query
    /// with <c>SQLite Error 1: no such column</c>, which only names the fix to somebody who already
    /// knows what it is. Stamping the version at creation and checking it here turns that into a
    /// startup message that says what happened and what to do.
    /// </remarks>
    public static async Task EnsureCreatedAsync(
        this TrackingContext context,
        CancellationToken cancellationToken = default
    )
    {
        var created = await context.Database.EnsureCreatedAsync(cancellationToken);

        if (created)
        {
            await StampVersionAsync(context, cancellationToken);
            return;
        }

        var found = await ReadVersionAsync(context, cancellationToken);

        // Zero is what every file written before this check existed reports, and those files match
        // the current model — the version was introduced alongside it, not after a change. Adopting
        // them is right; treating them as stale would refuse to start on a database that is fine.
        if (found == 0)
        {
            await StampVersionAsync(context, cancellationToken);
            return;
        }

        if (found != SchemaVersion)
            throw new InvalidOperationException(
                $"The tracking database was created by schema version {found}, but this build "
                    + $"expects {SchemaVersion}. There are no migrations for this app by design: "
                    + "move the existing database file aside and start again to have it recreated, "
                    + "keeping the old file if the data in it still matters."
            );
    }

    /// <summary>
    /// <c>user_version</c> is a four-byte slot in the SQLite header that the database itself never
    /// uses, which is exactly what makes it the right place: no table to create, nothing to migrate,
    /// and it survives anything that copies the file.
    /// </summary>
    private static async Task StampVersionAsync(
        TrackingContext context,
        CancellationToken cancellationToken
    ) =>
        // Interpolation rather than a parameter because PRAGMA does not accept one. The value is a
        // compile-time constant, so there is nothing here to inject.
        await context.Database.ExecuteSqlRawAsync(
            $"PRAGMA user_version = {SchemaVersion};",
            cancellationToken
        );

    private static async Task<int> ReadVersionAsync(
        TrackingContext context,
        CancellationToken cancellationToken
    )
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "PRAGMA user_version;";

        await context.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            var value = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(value);
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }
}
