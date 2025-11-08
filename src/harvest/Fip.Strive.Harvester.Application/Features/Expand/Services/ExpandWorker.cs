using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Queue.Application.Tasks;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services;

public class ExpandWorker(IExpansionService expander) : QueueWorker((int)SignalTypes.ImportSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<ImportSignal>(job);
        await expander.UnpackZipFileAsync(signal, ct);
    }
}
