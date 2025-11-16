using Fip.Strive.Harvester.Application.Core.Queue;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services;

public class ExpandWorker(IExpandService unzipper) : SignalQueueWorker(SignalTypes.ImportSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<ImportSignal>(job);
        await unzipper.ExpandAsync(signal, ct);
    }
}
