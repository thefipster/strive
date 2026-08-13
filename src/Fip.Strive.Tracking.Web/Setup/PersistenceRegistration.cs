using Fip.Strive.Tracking.Application.Features.Storage;
using Fip.Strive.Tracking.Application.Infrastructure;

namespace Fip.Strive.Tracking.Web.Setup;

public static class PersistenceRegistration
{
    /// <summary>
    /// Creates the SQLite file and its tables if they are not there yet, and logs where they ended
    /// up — so an unwritable or surprising data directory surfaces at startup instead of at the
    /// first click, and nobody has to guess what <c>Tracking__DataDirectory</c> actually resolved
    /// to.
    /// </summary>
    public static async Task EnsureDatabaseReadyAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<TrackingContext>();
        await context.EnsureCreatedAsync();

        var database = scope.ServiceProvider.GetRequiredService<TrackingDatabase>();
        app.Logger.LogInformation("Tracking database is {DatabaseFile}", database.FilePath);
    }
}
