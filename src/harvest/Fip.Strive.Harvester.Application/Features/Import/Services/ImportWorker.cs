using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Components;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Import.Services;

public class ImportWorker(IImportService inventory) : QueueWorker(SignalTypes.UploadSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<UploadSignal>(job);
        await inventory.ProcessUploadAsync(signal, ct);
    }
}
