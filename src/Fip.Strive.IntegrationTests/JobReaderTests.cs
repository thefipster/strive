using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class JobReaderTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Counts_are_reported_per_state()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        await harness.SeedAsync("noop", "a", JobState.Pending);
        await harness.SeedAsync("noop", "b", JobState.Pending);
        await harness.SeedAsync("noop", "c", JobState.Running);
        await harness.SeedAsync("noop", "d", JobState.Failed);

        await using var scope = harness.Scope();
        var counts = await Reader(scope).GetCountsAsync();

        counts.Pending.Should().Be(2);
        counts.Running.Should().Be(1);
        counts.Succeeded.Should().Be(0);
        counts.Failed.Should().Be(1);
    }

    [Fact]
    public async Task Unfinished_jobs_are_listed_before_finished_ones()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        await harness.SeedAsync(
            "noop",
            "old-success",
            JobState.Succeeded,
            enqueued: DateTimeOffset.UtcNow.AddHours(-2)
        );
        await harness.SeedAsync(
            "noop",
            "waiting",
            JobState.Pending,
            enqueued: DateTimeOffset.UtcNow.AddHours(-1)
        );

        await using var scope = harness.Scope();
        var jobs = await Reader(scope).GetJobsAsync();

        // What is happening now is what someone opened the page to see.
        jobs.Select(job => job.TargetKey).Should().ContainInOrder("waiting", "old-success");
    }

    [Fact]
    public async Task The_listing_is_capped()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        for (var index = 0; index < 30; index++)
            await harness.SeedAsync("noop", $"target-{index}", JobState.Succeeded);

        await using var scope = harness.Scope();

        (await Reader(scope).GetJobsAsync(10)).Should().HaveCount(10);
    }

    private static IJobReader Reader(AsyncServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IJobReader>();
}
