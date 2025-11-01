using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Components;
using Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services;

public class AssimilateClassifyWorker(IAssimilationService assimilator)
    : QueueWorker(SignalTypes.TypedSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<TypedSignal>(job);
        await assimilator.ExtractFileAsync(signal, ct);
    }
}
