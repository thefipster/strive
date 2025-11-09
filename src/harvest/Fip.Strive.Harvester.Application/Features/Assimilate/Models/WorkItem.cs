using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Models;

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
