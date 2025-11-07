using Fip.Strive.Queue.Application.Components;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Queue.Application.UnitTests;

public class TestQueueWorker() : QueueWorker(TestType)
{
    public const int TestType = 1337;

    public int Number { get; set; }

    public override Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var payload = GetSafePayload<TestPayload>(job);
        Number = payload.Number;
        return Task.CompletedTask;
    }
}
