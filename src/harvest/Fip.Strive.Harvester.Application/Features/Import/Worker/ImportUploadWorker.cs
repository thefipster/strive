using System.Text.Json;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Exceptions;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Import.Worker;

public class ImportUploadWorker(IZipInventory inventory) : ISignalQueueWorker
{
    public SignalTypes Type => SignalTypes.UploadSignal;

    public async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(job.Payload))
            throw new InvalidJobException(job, "Payload is null or empty.");

        var signal = JsonSerializer.Deserialize<UploadSignal>(job.Payload);
        if (signal == null)
            throw new InvalidJobException(job, "Can't read payload.");

        await inventory.ImportAsync(signal, ct);
    }
}
