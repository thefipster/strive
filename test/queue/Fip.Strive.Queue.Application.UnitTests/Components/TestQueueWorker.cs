using Fip.Strive.Queue.Application.Tasks;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Queue.Application.UnitTests.Components;

public class TestQueueWorker() : QueueWorker(TestType)
{
    public const int TestType = 1337;

    public int Number { get; set; }

    public override Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var payload = GetSafePayload<TestSignal>(job);
        Number = payload.Number;
        return Task.CompletedTask;
    }
}
