using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Components;
using Fip.Strive.Harvester.Application.Features.Classify.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Classify.Workers;

public class ClassifyExpandWorker(IScanner scanner) : QueueWorker(SignalTypes.FileSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<FileSignal>(job);
        await scanner.ClassifyAsync(signal, ct);
    }
}
