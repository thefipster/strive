using System.Collections.Concurrent;
using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class JobRunnerTests(PostgresFixture postgres)
{
    [Fact]
    public async Task An_enqueued_job_runs_and_succeeds()
    {
        var handler = new RecordingHandler();
        await using var harness = await JobHarness.CreateAsync(postgres, handler);

        await harness.EnqueueAsync("noop", "target-1");
        await harness.RunUntilIdleAsync();

        handler.Targets.Should().BeEquivalentTo(["target-1"]);

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Succeeded);
        job.FinishedUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task A_throwing_handler_parks_the_job_with_its_error()
    {
        await using var harness = await JobHarness.CreateAsync(
            postgres,
            new ThrowingHandler("the reader exploded")
        );

        await harness.EnqueueAsync("noop", "target-1");
        await harness.RunUntilIdleAsync();

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Failed);
        job.Error.Should().Contain("the reader exploded");
        job.Attempts.Should().Be(1, "one attempt, then it waits for a human");
    }

    [Fact]
    public async Task A_job_left_running_by_a_previous_process_is_picked_up_on_start()
    {
        var handler = new RecordingHandler();
        await using var harness = await JobHarness.CreateAsync(postgres, handler);

        await harness.SeedAsync("noop", "interrupted", JobState.Running, attempts: 1);
        await harness.RunUntilIdleAsync();

        handler.Targets.Should().BeEquivalentTo(["interrupted"]);

        await using var reader = harness.CreateContext();
        (await reader.Jobs.SingleAsync()).State.Should().Be(JobState.Succeeded);
    }

    [Fact]
    public async Task A_disabled_runner_executes_nothing()
    {
        var handler = new RecordingHandler();
        await using var harness = await JobHarness.CreateAsync(postgres, handler);

        await harness.EnqueueAsync("noop", "target-1");
        await harness.RunUntilIdleAsync(TimeSpan.FromSeconds(2), enabled: false);

        handler.Targets.Should().BeEmpty();

        await using var reader = harness.CreateContext();
        (await reader.Jobs.SingleAsync()).State.Should().Be(JobState.Pending);
    }

    [Fact]
    public async Task Progress_reported_by_a_handler_reaches_the_table()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new ProgressHandler());

        await harness.EnqueueAsync("noop", "target-1");
        await harness.RunUntilIdleAsync();

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        // The throttle holds the middle reports back; the flush before the terminal write is what
        // makes the last one land, so a finished job does not sit displaying 1 of 40.
        job.ProgressCurrent.Should().Be(40);
        job.ProgressTotal.Should().Be(40);
    }

    [Fact]
    public async Task Many_jobs_all_run_exactly_once()
    {
        var handler = new RecordingHandler();
        await using var harness = await JobHarness.CreateAsync(postgres, handler);

        for (var index = 0; index < 50; index++)
            await harness.EnqueueAsync("noop", $"target-{index}");

        await harness.RunUntilIdleAsync();

        handler.Targets.Should().HaveCount(50);
        handler.Targets.Distinct().Should().HaveCount(50, "no job may run twice");

        await using var reader = harness.CreateContext();
        (await reader.Jobs.CountAsync(job => job.State == JobState.Succeeded)).Should().Be(50);
    }

    private sealed class RecordingHandler : IJobHandler
    {
        private readonly ConcurrentBag<string> _targets = [];

        public string Kind => "noop";

        public string ComponentId => "noop";

        public int Version => 1;

        public IReadOnlyCollection<string> Targets => [.. _targets];

        public Task ExecuteAsync(JobContext context, CancellationToken cancellationToken)
        {
            _targets.Add(context.Job.TargetKey);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler(string message) : IJobHandler
    {
        public string Kind => "noop";

        public string ComponentId => "noop";

        public int Version => 1;

        public Task ExecuteAsync(JobContext context, CancellationToken cancellationToken) =>
            throw new InvalidOperationException(message);
    }

    private sealed class ProgressHandler : IJobHandler
    {
        public string Kind => "noop";

        public string ComponentId => "noop";

        public int Version => 1;

        public Task ExecuteAsync(JobContext context, CancellationToken cancellationToken)
        {
            for (var index = 1; index <= 40; index++)
                context.Progress.Report(new JobProgress(index, 40, $"file-{index}.json"));

            return Task.CompletedTask;
        }
    }
}
