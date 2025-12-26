using Fip.Strive.Core.Application.Features.PubSub.Models;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Queue.Application.UnitTests.Components;

public class TestSignal(int number) : Signal(TestQueueWorker.TestType)
{
    public int Number { get; set; } = number;
}
