using Npgsql;
using Testcontainers.PostgreSql;

namespace Fip.Strive.IntegrationTests.Fixtures;

/// <summary>
/// One throwaway Postgres server for the whole assembly. Tests take a freshly created database
/// off it so they stay isolated without paying for a container each.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    // Same major as the Aspire AppHost runs in development, so tests and dev agree.
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(
        "postgres:18-alpine"
    ).Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public async Task<string> CreateDatabaseAsync()
    {
        var name = $"strive_{Guid.NewGuid():n}";

        await using (var connection = new NpgsqlConnection(_container.GetConnectionString()))
        {
            await connection.OpenAsync();

            // The name is a generated GUID, never test input, so interpolation is safe here —
            // and identifiers cannot be parameterised anyway.
            await using var command = new NpgsqlCommand($"""CREATE DATABASE "{name}" """, connection);
            await command.ExecuteNonQueryAsync();
        }

        return new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = name,
        }.ConnectionString;
    }
}

[CollectionDefinition(Name)]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}
