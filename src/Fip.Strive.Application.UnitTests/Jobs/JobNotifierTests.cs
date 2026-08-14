using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fip.Strive.Application.UnitTests.Jobs;

public class JobNotifierTests
{
    [Fact]
    public void Subscribers_are_told_something_changed()
    {
        var notifier = new JobNotifier(NullLogger<JobNotifier>.Instance);
        var calls = 0;

        using var subscription = notifier.Subscribe(() => calls++);
        notifier.Notify();

        calls.Should().Be(1);
    }

    [Fact]
    public void Disposing_a_subscription_stops_the_callbacks()
    {
        var notifier = new JobNotifier(NullLogger<JobNotifier>.Instance);
        var calls = 0;

        var subscription = notifier.Subscribe(() => calls++);
        subscription.Dispose();
        notifier.Notify();

        calls.Should().Be(0);
    }

    [Fact]
    public void One_faulted_subscriber_does_not_stop_the_others()
    {
        var notifier = new JobNotifier(NullLogger<JobNotifier>.Instance);
        var reached = false;

        using var bad = notifier.Subscribe(() =>
            throw new InvalidOperationException("circuit is gone")
        );
        using var good = notifier.Subscribe(() => reached = true);

        notifier.Notify();

        // A dead browser circuit must never be able to stall the runner.
        reached.Should().BeTrue();
    }
}
