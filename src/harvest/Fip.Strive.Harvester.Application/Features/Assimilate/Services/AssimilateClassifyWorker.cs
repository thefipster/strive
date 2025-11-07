using Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Queue.Application.Components;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services;

public class AssimilateClassifyWorker(IAssimilationService assimilator)
    : QueueWorker((int)SignalTypes.TypedSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<TypedSignal>(job);
        await assimilator.ExtractFileAsync(signal, ct);
    }
}
