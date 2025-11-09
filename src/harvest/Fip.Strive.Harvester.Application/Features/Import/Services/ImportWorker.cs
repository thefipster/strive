using Fip.Strive.Harvester.Application.Core.Queue;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Harvester.Application.Features.Import.Services;

public class ImportWorker(IImportService importService)
    : SignalQueueWorker(SignalTypes.UploadSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<UploadSignal>(job);
        await importService.MoveZipAsync(signal, ct);
    }
}
