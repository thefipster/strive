namespace Fip.Strive.Tracking.Application.Features.Storage;

public sealed class TrackingStorageOptions
{
    public const string SectionName = "Tracking";

    /// <summary>
    /// Directory holding the database file. A relative path resolves against the host's content
    /// root; an absolute path is used as-is. Override in a container with
    /// <c>Tracking__DataDirectory</c>.
    /// </summary>
    public string DataDirectory { get; set; } = "data";

    /// <summary>
    /// Name of the SQLite file inside <see cref="DataDirectory"/>. Everything the app knows lives
    /// in this one file — copy it away and that is your backup.
    /// </summary>
    public string DatabaseFileName { get; set; } = "tracking.db";
}
