using Fip.Strive.Harvester.Application.Features.Classify.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Queue.Application.Components;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Harvester.Application.Features.Classify.Services;

public class ClassifyExpandWorker(IScanner scanner) : QueueWorker((int)SignalTypes.FileSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<FileSignal>(job);
        await scanner.ClassifyAsync(signal, ct);
    }
}
