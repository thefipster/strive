using Microsoft.Data.Sqlite;

namespace Fip.Strive.Tracking.Application.Features.Storage;

/// <summary>
/// The resolved location of the SQLite file, built once at startup so nothing downstream has to
/// reason about relative-vs-absolute configuration again.
/// </summary>
public sealed class TrackingDatabase
{
    public TrackingDatabase(string directoryPath, string fileName)
    {
        DirectoryPath = directoryPath;
        FilePath = Path.Combine(directoryPath, fileName);
    }

    /// <summary>Configured data directory, always absolute.</summary>
    public string DirectoryPath { get; }

    public string FilePath { get; }

    /// <summary>
    /// Built rather than interpolated so a path containing a space, a quote or a semicolon does not
    /// produce a connection string that means something else entirely.
    /// </summary>
    public string ConnectionString =>
        new SqliteConnectionStringBuilder { DataSource = FilePath }.ConnectionString;

    /// <summary>
    /// SQLite creates the file but not the folder above it, so this has to happen before the first
    /// connection is opened.
    /// </summary>
    public void EnsureDirectoryCreated() => Directory.CreateDirectory(DirectoryPath);
}
