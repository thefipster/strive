using Fip.Strive.Tracking.Application.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Tracking.Application.UnitTests.Fixtures;

/// <summary>
/// A throwaway SQLite file per test. A real file rather than an in-memory database, because that is
/// what the app runs on and it is the file that decides how a decimal or a
/// <see cref="DateTimeOffset"/> comes back out.
/// </summary>
public sealed class TrackingDatabaseFixture : IDisposable
{
    private readonly string _directory;
    private readonly List<TrackingContext> _contexts = [];

    public TrackingDatabaseFixture()
    {
        _directory = Path.Combine(
            Path.GetTempPath(),
            "tracking-tests",
            Guid.NewGuid().ToString("n")
        );

        Directory.CreateDirectory(_directory);
        ConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(_directory, "tracking.db"),
        }.ConnectionString;

        using var context = NewContext();
        context.Database.EnsureCreated();
    }

    private string ConnectionString { get; }

    /// <summary>
    /// A fresh context on the same file, so a test can write through one and assert through another
    /// without the change tracker answering from memory.
    /// </summary>
    public TrackingContext NewContext()
    {
        var options = new DbContextOptionsBuilder<TrackingContext>()
            .UseSqlite(ConnectionString)
            .Options;

        var context = new TrackingContext(options);
        _contexts.Add(context);

        return context;
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
            context.Dispose();

        // Pooled connections keep the file open on Windows, and an open file cannot be deleted.
        SqliteConnection.ClearAllPools();

        try
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
            // A leaked temp directory is not worth failing a green test over.
        }
    }
}
