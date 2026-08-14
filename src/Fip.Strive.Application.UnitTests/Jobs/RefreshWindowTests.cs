using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Services;

namespace Fip.Strive.Application.UnitTests.Jobs;

public class RefreshWindowTests
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(200);
    private static readonly DateTimeOffset Start = DateTimeOffset.UnixEpoch;

    [Fact]
    public void The_first_request_refreshes_straight_away()
    {
        var window = new RefreshWindow(Interval);

        window.Request(Start).Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void A_request_inside_the_window_waits_out_the_remainder()
    {
        var window = new RefreshWindow(Interval);

        window.Request(Start);
        window.Completed(Start);

        window
            .Request(Start + TimeSpan.FromMilliseconds(50))
            .Should()
            .Be(TimeSpan.FromMilliseconds(150));
    }

    [Fact]
    public void Requests_arriving_while_one_is_scheduled_fold_into_it()
    {
        var window = new RefreshWindow(Interval);

        window.Request(Start);

        window.Request(Start + TimeSpan.FromMilliseconds(10)).Should().BeNull();
        window.Request(Start + TimeSpan.FromMilliseconds(20)).Should().BeNull();
    }

    [Fact]
    public void The_final_notification_of_a_burst_is_never_lost()
    {
        var window = new RefreshWindow(Interval);
        var refreshes = 0;

        // A running job's progress reports, then the one that says it finished.
        for (var tick = 0; tick <= 5; tick++)
        {
            var now = Start + TimeSpan.FromMilliseconds(tick * 10);

            if (window.Request(now) is not { } delay)
                continue;

            refreshes++;
            window.Completed(now + delay);
        }

        // A leading-edge-only throttle would stop at one and leave the job displayed as running.
        refreshes
            .Should()
            .BeGreaterThan(1, "the burst must end in a read that sees the terminal state");
    }

    [Fact]
    public void A_request_after_the_window_has_passed_refreshes_straight_away()
    {
        var window = new RefreshWindow(Interval);

        window.Request(Start);
        window.Completed(Start);

        window.Request(Start + Interval).Should().Be(TimeSpan.Zero);
    }
}
