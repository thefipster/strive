using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class JobQueueTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Enqueueing_writes_a_pending_row_stamped_with_the_component()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(3));

        await harness.EnqueueAsync("noop", "target-1");

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Pending);
        job.Kind.Should().Be("noop");
        job.TargetKey.Should().Be("target-1");
        job.ComponentId.Should().Be("noop");
        job.ComponentVersion.Should().Be(3);
        job.Attempts.Should().Be(0);
        job.StartedUtc.Should().BeNull();
    }

    [Fact]
    public async Task Enqueueing_a_known_unit_updates_it_instead_of_adding_a_row()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        await harness.EnqueueAsync("noop", "target-1");

        await using (var context = harness.CreateContext())
        {
            var failed = await context.Jobs.SingleAsync();
            failed.State = JobState.Failed;
            failed.Error = "it went wrong";
            failed.FinishedUtc = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();
        }

        await harness.EnqueueAsync("noop", "target-1");

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Pending);
        job.Error.Should().BeNull("a re-queued unit must not display the last run's failure");
        job.FinishedUtc.Should().BeNull();
    }

    [Fact]
    public async Task The_payload_round_trips_as_json()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        await harness.EnqueueAsync(
            "noop",
            "target-1",
            new { Path = "/tmp/a.zip", SizeBytes = 42L }
        );

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.Payload.Should().Contain("\"path\"").And.Contain("/tmp/a.zip");
    }

    [Fact]
    public async Task Re_queueing_without_a_payload_keeps_the_one_already_stored()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        await harness.EnqueueAsync("noop", "target-1", new { Path = "/tmp/a.zip" });
        await harness.EnqueueAsync("noop", "target-1");

        await using var reader = harness.CreateContext();

        // The jobs page's retry button has no payload to give. Nulling it here would strand an
        // unpack job's only record of where its archive is.
        (await reader.Jobs.SingleAsync())
            .Payload.Should()
            .Contain("/tmp/a.zip");
    }

    [Fact]
    public async Task Enqueueing_a_unit_that_is_already_running_leaves_it_alone()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        var id = await harness.SeedAsync("noop", "target-1", JobState.Running, attempts: 1);

        var result = await harness.EnqueueAsync("noop", "target-1", new { Path = "/tmp/b.zip" });

        result.JobId.Should().Be(id);
        result.Outcome.Should().Be(EnqueueOutcome.AlreadyRunning);

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        // Putting a claimed row back to Pending does not interrupt the worker holding it; it
        // simply lets the pump hand the same unit to a second one.
        job.State.Should().Be(JobState.Running);
        job.StartedUtc.Should().NotBeNull();
        job.Payload.Should().BeNull("the run under way keeps the input it started with");
    }

    [Fact]
    public async Task Enqueueing_a_settled_unit_reports_it_as_re_queued()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        var id = await harness.SeedAsync("noop", "target-1", JobState.Failed);

        var result = await harness.EnqueueAsync("noop", "target-1");

        result.JobId.Should().Be(id);
        result.Outcome.Should().Be(EnqueueOutcome.Requeued);
    }

    [Fact]
    public async Task Losing_the_insert_race_settles_on_the_winner_s_row()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        await using var blocker = harness.CreateContext();
        await using var transaction = await blocker.Database.BeginTransactionAsync();

        // The same unit, inserted by another writer and not yet committed. Read committed hides
        // the row from the enqueue below, so it finds nothing to update and inserts too — and then
        // waits on the unique index until this transaction says which of them won.
        blocker.Jobs.Add(
            new Job
            {
                Id = Guid.CreateVersion7(),
                Kind = "noop",
                TargetKey = "target-1",
                ComponentId = "noop",
                ComponentVersion = 1,
                State = JobState.Pending,
                EnqueuedUtc = DateTimeOffset.UtcNow,
            }
        );

        await blocker.SaveChangesAsync();

        var enqueue = harness.EnqueueAsync("noop", "target-1", new { Path = "/tmp/a.zip" });

        await Task.Delay(TimeSpan.FromMilliseconds(500));
        enqueue.IsCompleted.Should().BeFalse("it is blocked on the other writer's index entry");

        await transaction.CommitAsync();

        // Losing is not failing: the work unit the caller asked for exists, which is all it wanted.
        (await enqueue)
            .Outcome.Should()
            .Be(EnqueueOutcome.Requeued);

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.Payload.Should().Contain("/tmp/a.zip", "the retry applied the payload it was given");
    }

    [Fact]
    public async Task Enqueueing_an_unregistered_kind_is_refused()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        var act = async () => await harness.EnqueueAsync("classify", "target-1");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Enqueueing_wakes_the_pump()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        await harness.EnqueueAsync("noop", "target-1");

        // Already signalled, so this returns without waiting out the timeout.
        await harness
            .Resolve<JobSignal>()
            .WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
    }

    private sealed class NoopHandler(int version) : IJobHandler
    {
        public string Kind => "noop";

        public string ComponentId => "noop";

        public int Version => version;

        public Task ExecuteAsync(JobContext context, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
