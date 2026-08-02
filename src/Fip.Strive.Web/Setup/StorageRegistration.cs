using Fip.Strive.Application.Features.Storage;

namespace Fip.Strive.Web.Setup;

public static class StorageRegistration
{
    /// <summary>
    /// Resolves the storage layout at startup so a missing or unwritable data directory surfaces
    /// immediately instead of at the first upload — and so the resolved path is in the log when
    /// someone wonders where <c>Storage__DataDirectory</c> actually pointed.
    /// </summary>
    public static void EnsureStorageReady(this WebApplication app)
    {
        var paths = app.Services.GetRequiredService<StoragePaths>();
        app.Logger.LogInformation("Data directory is {DataDirectory}", paths.Root);
    }
}
