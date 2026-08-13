using AwesomeAssertions;
using Fip.Strive.Tracking.Application.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Tracking.Application.UnitTests.Infrastructure;

/// <summary>
/// The version stamp exists for one moment: the first start after a model change, on a file that
/// cannot satisfy it. Without it the app starts happily and fails later at whichever page happens
/// to touch the missing column first.
/// </summary>
/// <remarks>
/// Manages its own file rather than using <c>TrackingDatabaseFixture</c>, which creates the schema
/// in its constructor — these tests need to decide for themselves whether the database exists yet.
/// </remarks>
public sealed class TrackingSchemaTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        "tracking-schema-tests",
        Guid.NewGuid().ToString("n")
    );

    private readonly List<TrackingContext> _contexts = [];
    private readonly string _connectionString;

    public TrackingSchemaTests()
    {
        Directory.CreateDirectory(_directory);

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(_directory, "tracking.db"),
        }.ConnectionString;
    }

    [Fact]
    public async Task Creating_the_database_stamps_the_current_version()
    {
        var context = NewContext();
        await context.EnsureCreatedAsync();

        (await ReadVersionAsync(context)).Should().Be(TrackingSchema.SchemaVersion);
    }

    [Fact]
    public async Task Starting_again_on_a_matching_database_is_a_no_op()
    {
        await NewContext().EnsureCreatedAsync();

        var act = async () => await NewContext().EnsureCreatedAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task A_file_written_before_versioning_existed_is_adopted()
    {
        var first = NewContext();
        await first.EnsureCreatedAsync();

        // What every database created before this check reports. Those files match the current
        // model, so refusing to start on one would be refusing a database that is fine.
        await first.Database.ExecuteSqlRawAsync("PRAGMA user_version = 0;");

        var second = NewContext();
        var act = async () => await second.EnsureCreatedAsync();

        await act.Should().NotThrowAsync();
        (await ReadVersionAsync(second)).Should().Be(TrackingSchema.SchemaVersion);
    }

    [Fact]
    public async Task A_database_from_another_schema_version_fails_with_an_actionable_message()
    {
        var first = NewContext();
        await first.EnsureCreatedAsync();
        await first.Database.ExecuteSqlRawAsync("PRAGMA user_version = 99;");

        var act = async () => await NewContext().EnsureCreatedAsync();

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*99*")
            .WithMessage("*move the existing database file aside*");
    }

    [Fact]
    public async Task The_export_index_is_created_on_a_fresh_database()
    {
        var context = NewContext();
        await context.EnsureCreatedAsync();

        (await IndexNamesAsync(context)).Should().Contain("IX_tracker_events_RecordedUtc_Id");
    }

    [Fact]
    public async Task The_export_index_is_added_to_a_database_that_predates_it()
    {
        var first = NewContext();
        await first.EnsureCreatedAsync();

        // A database from before the index existed. Dropping it is the only way to reach that
        // state, since EnsureCreated builds it from the current model.
        await first.Database.ExecuteSqlRawAsync(
            "DROP INDEX IF EXISTS \"IX_tracker_events_RecordedUtc_Id\";"
        );

        var second = NewContext();
        await second.EnsureCreatedAsync();

        // An index is derived data, so it is added in place rather than demanding the file be moved
        // aside — which for a performance index would mean discarding the user's data.
        (await IndexNamesAsync(second))
            .Should()
            .Contain("IX_tracker_events_RecordedUtc_Id");
    }

    private static async Task<List<string>> IndexNamesAsync(TrackingContext context)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'index';";

        await context.Database.OpenConnectionAsync();

        try
        {
            var names = new List<string>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                names.Add(reader.GetString(0));

            return names;
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    private TrackingContext NewContext()
    {
        var context = new TrackingContext(
            new DbContextOptionsBuilder<TrackingContext>().UseSqlite(_connectionString).Options
        );

        _contexts.Add(context);
        return context;
    }

    private static async Task<int> ReadVersionAsync(TrackingContext context)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "PRAGMA user_version;";

        await context.Database.OpenConnectionAsync();

        try
        {
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
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
