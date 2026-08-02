using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Web.Setup;

public static class PersistenceRegistration
{
    /// <summary>
    /// Brings the schema up to date at startup. Single-user homelab app with one writer, so there
    /// is no window where two versions of the app race over migrations.
    /// </summary>
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<StriveContext>();
        await context.Database.MigrateAsync();
    }
}
