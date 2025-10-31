using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Models;

public class WorkItem
{
    public required TypedSignal Signal { get; set; }
    public List<FileExtraction> Extractions { get; set; } = [];

    public static WorkItem FromSignal(TypedSignal signal)
    {
        return new WorkItem { Signal = signal };
    }
}
