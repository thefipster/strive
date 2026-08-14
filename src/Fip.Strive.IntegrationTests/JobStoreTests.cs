using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class JobStoreTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Claiming_marks_the_job_running_and_counts_the_attempt()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);
        var id = await harness.SeedAsync("noop", "target-1", JobState.Pending);

        await using var scope = harness.Scope();
        var claimed = await Store(scope).ClaimAsync(10, CancellationToken.None);

        claimed.Should().BeEquivalentTo([id]);

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Running);
        job.Attempts.Should().Be(1);
        job.StartedUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Claiming_takes_no_more_than_it_asked_for_and_takes_the_oldest_first()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        var first = await harness.SeedAsync(
            "noop",
            "a",
            JobState.Pending,
            enqueued: DateTimeOffset.UtcNow.AddMinutes(-5)
        );
        await harness.SeedAsync(
            "noop",
            "b",
            JobState.Pending,
            enqueued: DateTimeOffset.UtcNow.AddMinutes(-1)
        );

        await using var scope = harness.Scope();

        (await Store(scope).ClaimAsync(1, CancellationToken.None)).Should().BeEquivalentTo([first]);
    }

    [Fact]
    public async Task Two_claims_never_hand_the_same_job_to_two_workers()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        for (var index = 0; index < 20; index++)
            await harness.SeedAsync("noop", $"target-{index}", JobState.Pending);

        await using var one = harness.Scope();
        await using var two = harness.Scope();

        // Run together on separate connections: SKIP LOCKED is what has to keep them disjoint.
        var results = await Task.WhenAll(
            Store(one).ClaimAsync(20, CancellationToken.None),
            Store(two).ClaimAsync(20, CancellationToken.None)
        );

        var all = results.SelectMany(ids => ids).ToList();

        all.Should().HaveCount(20);
        all.Distinct().Should().HaveCount(20, "no job may be claimed twice");
    }

    [Fact]
    public async Task Only_pending_jobs_are_claimed()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        await harness.SeedAsync("noop", "running", JobState.Running);
        await harness.SeedAsync("noop", "succeeded", JobState.Succeeded);
        await harness.SeedAsync("noop", "failed", JobState.Failed);

        await using var scope = harness.Scope();

        (await Store(scope).ClaimAsync(10, CancellationToken.None)).Should().BeEmpty();
    }

    [Fact]
    public async Task Recovery_requeues_interrupted_and_stale_jobs_without_spending_an_attempt()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        var interrupted = await harness.SeedAsync(
            "noop",
            "interrupted",
            JobState.Running,
            attempts: 1
        );
        await harness.SeedAsync("noop", "stale", JobState.Stale);
        await harness.SeedAsync("noop", "failed", JobState.Failed);

        await using var scope = harness.Scope();

        (await Store(scope).RecoverInterruptedAsync(CancellationToken.None)).Should().Be(2);

        await using var reader = harness.CreateContext();

        var job = await reader.Jobs.SingleAsync(row => row.Id == interrupted);
        job.State.Should().Be(JobState.Pending);
        job.StartedUtc.Should().BeNull();
        job.Attempts.Should().Be(1, "a kill is not a failed attempt");

        var failed = await reader.Jobs.SingleAsync(row => row.TargetKey == "failed");
        failed.State.Should().Be(JobState.Failed, "a parked failure waits for a manual retry");
    }

    [Fact]
    public async Task Completing_and_failing_write_the_terminal_state()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);
        var succeeded = await harness.SeedAsync("noop", "ok", JobState.Running);
        var failed = await harness.SeedAsync("noop", "bad", JobState.Running);

        await using (var scope = harness.Scope())
        {
            await Store(scope).CompleteAsync(succeeded, CancellationToken.None);
            await Store(scope).FailAsync(failed, "disk on fire", CancellationToken.None);
        }

        await using var reader = harness.CreateContext();

        var ok = await reader.Jobs.SingleAsync(row => row.Id == succeeded);
        ok.State.Should().Be(JobState.Succeeded);
        ok.FinishedUtc.Should().NotBeNull();
        ok.Error.Should().BeNull();

        var bad = await reader.Jobs.SingleAsync(row => row.Id == failed);
        bad.State.Should().Be(JobState.Failed);
        bad.Error.Should().Be("disk on fire");
        bad.FinishedUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Releasing_returns_a_job_to_the_queue_untouched()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);
        var id = await harness.SeedAsync("noop", "target-1", JobState.Running, attempts: 1);

        await using (var scope = harness.Scope())
            await Store(scope).ReleaseAsync(id, CancellationToken.None);

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Pending);
        job.Attempts.Should().Be(1, "shutdown is not a failed attempt");
        job.Error.Should().BeNull();
    }

    [Fact]
    public async Task Progress_is_stored_against_the_job()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);
        var id = await harness.SeedAsync("noop", "target-1", JobState.Running);

        await using (var scope = harness.Scope())
            await Store(scope)
                .SaveProgressAsync(id, new JobProgress(7, 40, "a/b.json"), CancellationToken.None);

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.ProgressCurrent.Should().Be(7);
        job.ProgressTotal.Should().Be(40);
        job.ProgressNote.Should().Be("a/b.json");
    }

    private static IJobStore Store(AsyncServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IJobStore>();
}
