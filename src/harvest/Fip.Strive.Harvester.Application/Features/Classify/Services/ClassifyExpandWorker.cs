using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Components;
using Fip.Strive.Harvester.Application.Features.Classify.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Application.Features.Classify.Services;

public class ClassifyExpandWorker(IScanner scanner) : QueueWorker(SignalTypes.FileSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<FileSignal>(job);
        await scanner.ClassifyAsync(signal, ct);
    }
}
