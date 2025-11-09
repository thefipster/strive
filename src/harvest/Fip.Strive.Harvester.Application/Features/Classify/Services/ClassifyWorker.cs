using Fip.Strive.Harvester.Application.Core.Queue;
using Fip.Strive.Harvester.Application.Features.Classify.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Harvester.Application.Features.Classify.Services;

public class ClassifyWorker(IClassifyService classifyService)
    : SignalQueueWorker(SignalTypes.FileSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<FileSignal>(job);
        await classifyService.ScanFile(signal, ct);
    }
}
