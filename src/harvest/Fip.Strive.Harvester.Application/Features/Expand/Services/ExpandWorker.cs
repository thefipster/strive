using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Components;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services;

public class ExpandWorker(IExpansionService expander) : QueueWorker(SignalTypes.ImportSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<ImportSignal>(job);
        await expander.UnpackZipFileAsync(signal, ct);
    }
}
