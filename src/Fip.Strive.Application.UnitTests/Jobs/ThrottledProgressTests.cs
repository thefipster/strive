using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services;
using Fip.Strive.Application.UnitTests.Fixtures;

namespace Fip.Strive.Application.UnitTests.Jobs;

public class ThrottledProgressTests
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void The_first_report_is_written_immediately()
    {
        var progress = Create(out var written, out _);

        progress.Report(new JobProgress(1, 10));

        written.Should().HaveCount(1);
    }

    [Fact]
    public void Reports_inside_the_interval_are_held_back()
    {
        var progress = Create(out var written, out var clock);

        progress.Report(new JobProgress(1, 10));
        clock.Advance(TimeSpan.FromMilliseconds(100));
        progress.Report(new JobProgress(2, 10));
        clock.Advance(TimeSpan.FromMilliseconds(100));
        progress.Report(new JobProgress(3, 10));

        // Forty thousand files must not become forty thousand UPDATEs.
        written.Should().HaveCount(1);
    }

    [Fact]
    public void A_report_after_the_interval_is_written()
    {
        var progress = Create(out var written, out var clock);

        progress.Report(new JobProgress(1, 10));
        clock.Advance(Interval);
        progress.Report(new JobProgress(9, 10));

        written.Should().HaveCount(2);
        written[1].Current.Should().Be(9);
    }

    [Fact]
    public async Task Flushing_writes_the_last_held_report()
    {
        var progress = Create(out var written, out var clock);

        progress.Report(new JobProgress(1, 10));
        clock.Advance(TimeSpan.FromMilliseconds(10));
        progress.Report(new JobProgress(10, 10));

        await progress.FlushAsync();

        // Without this a finished job would sit displaying 1 of 10 forever.
        written.Should().HaveCount(2);
        written[1].Current.Should().Be(10);
    }

    [Fact]
    public async Task Flushing_with_nothing_held_writes_nothing()
    {
        var progress = Create(out var written, out _);

        progress.Report(new JobProgress(1, 10));
        await progress.FlushAsync();
        await progress.FlushAsync();

        written.Should().HaveCount(1);
    }

    [Fact]
    public async Task Flushing_waits_for_a_write_that_is_still_in_flight()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var written = new List<JobProgress>();

        var progress = new ThrottledProgress(
            async value =>
            {
                await gate.Task;
                written.Add(value);
            },
            Interval,
            new StubClock(DateTimeOffset.UnixEpoch)
        );

        progress.Report(new JobProgress(1, 10));

        var flush = progress.FlushAsync();
        flush.IsCompleted.Should().BeFalse("the write it started has not landed yet");

        gate.SetResult();
        await flush;

        // The job's terminal state is written the moment this returns. A write still in flight
        // here would land on a row that has already finished.
        written.Should().HaveCount(1);
    }

    [Fact]
    public async Task Writes_land_in_the_order_they_were_reported()
    {
        var first = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var written = new List<JobProgress>();
        var clock = new StubClock(DateTimeOffset.UnixEpoch);

        var progress = new ThrottledProgress(
            async value =>
            {
                // Only the earlier write is held up. Started independently, the later one would
                // overtake it and leave the row displaying a position the job had already passed.
                if (value.Current == 1)
                    await first.Task;

                written.Add(value);
            },
            Interval,
            clock
        );

        progress.Report(new JobProgress(1, 10));
        clock.Advance(Interval);
        progress.Report(new JobProgress(2, 10));

        first.SetResult();
        await progress.FlushAsync();

        written.Select(value => value.Current).Should().Equal(1, 2);
    }

    [Fact]
    public void A_failing_write_does_not_reach_the_handler()
    {
        var progress = new ThrottledProgress(
            _ => throw new InvalidOperationException("the database is gone"),
            Interval,
            new StubClock(DateTimeOffset.UnixEpoch)
        );

        // Progress is advisory. A handler must not die because its position could not be recorded.
        var act = () => progress.Report(new JobProgress(1, 10));

        act.Should().NotThrow();
    }

    private static ThrottledProgress Create(out List<JobProgress> written, out StubClock clock)
    {
        var captured = new List<JobProgress>();
        written = captured;
        clock = new StubClock(DateTimeOffset.UnixEpoch);

        // Completes synchronously, so assertions do not have to wait on the fire-and-forget path.
        return new ThrottledProgress(
            value =>
            {
                captured.Add(value);
                return Task.CompletedTask;
            },
            Interval,
            clock
        );
    }
}
