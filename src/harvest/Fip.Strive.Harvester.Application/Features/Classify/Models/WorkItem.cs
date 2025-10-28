using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Harvester.Application.Features.Classify.Models;

public class WorkItem
{
    public required FileSignal Signal { get; set; }

    public static WorkItem FromSignal(FileSignal signal)
    {
        return new WorkItem { Signal = signal };
    }
}
