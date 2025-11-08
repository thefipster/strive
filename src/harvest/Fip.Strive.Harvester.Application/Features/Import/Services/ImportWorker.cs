using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Queue.Application.Components;
using Fip.Strive.Queue.Application.Tasks;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Harvester.Application.Features.Import.Services;

public class ImportWorker(IImportService inventory) : QueueWorker((int)SignalTypes.UploadSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<UploadSignal>(job);
        await inventory.ProcessUploadAsync(signal, ct);
    }
}
