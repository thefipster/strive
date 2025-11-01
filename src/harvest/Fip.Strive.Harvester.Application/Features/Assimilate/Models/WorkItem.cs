using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Models;

public class WorkItem
{
    public required TypedSignal Signal { get; set; }
    public List<FileExtraction> Extractions { get; set; } = [];
    public DataIndex? Index { get; set; }
    public IFileExtractor? Extractor { get; set; }

    public static WorkItem FromSignal(TypedSignal signal)
    {
        return new WorkItem { Signal = signal };
    }
}
