using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class JobSchemaTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Migrations_produce_an_empty_job_table()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);
        await using var context = harness.CreateContext();

        (await context.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        (await context.Jobs.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task A_work_unit_can_only_exist_once()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);
        await using var context = harness.CreateContext();

        context.Jobs.Add(NewJob());
        await context.SaveChangesAsync();

        context.Jobs.Add(NewJob());

        // The unique index is what makes an enqueue an upsert rather than an append; without it a
        // replay would grow a row per run.
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task State_is_stored_as_its_name_not_its_ordinal()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        await using (var context = harness.CreateContext())
        {
            context.Jobs.Add(NewJob());
            await context.SaveChangesAsync();
        }

        await using var reader = harness.CreateContext();

        // Quoted PascalCase: this repository maps tables to snake_case but leaves columns as the
        // property names, and Postgres folds an unquoted identifier to lowercase.
        var stored = await reader
            .Database.SqlQueryRaw<string>("SELECT \"State\" AS \"Value\" FROM jobs")
            .SingleAsync();

        stored.Should().Be("Pending", "reordering the enum must never reinterpret existing rows");
    }

    private static Job NewJob() =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Kind = "unpack",
            TargetKey = "abc123",
            ComponentId = "unpack",
            ComponentVersion = 1,
            State = JobState.Pending,
            EnqueuedUtc = DateTimeOffset.UtcNow,
        };
}
