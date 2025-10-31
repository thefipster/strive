using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Core.Ingestion.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Assimilate.Models;
using Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services;

public class AssimilationService(IExtractor extractor) : IAssimilationService
{
    public async Task<WorkItem> ExtractFileAsync(TypedSignal signal, CancellationToken ct)
    {
        WorkItem work = WorkItem.FromSignal(signal);

        // check file index for assimilation and version

        work.Extractions = await extractor.ExtractAsync(
            work.Signal.Filepath,
            work.Signal.Source,
            work.Signal.Timestamp
        );
        foreach (var extraction in work.Extractions)
            HandleExtraction(extraction, work);

        // update file index with assimilation info

        return work;
    }

    private void HandleExtraction(FileExtraction extraction, WorkItem work)
    {
        // save extraction to disc

        // update assimilation index

        // create inventory

        // profit

        throw new NotImplementedException();
    }
}
