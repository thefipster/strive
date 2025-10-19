using System.Text.Json;
using Fip.Strive.Core.Domain.Extensions;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Enums;
using Fip.Strive.Harvester.Application.Core.Queue.Exceptions;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Features.Upload.Signals;

namespace Fip.Strive.Harvester.Application.Features.Importer.Worker;

public class ImportUploadWorker : ISignalQueueWorker
{
    public SignalTypes Type => SignalTypes.UploadSignal;

    public async Task ProcessAsync(JobEntity job, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(job.Payload))
            throw new InvalidJobException(job, "Payload is null or empty.");

        var signal = JsonSerializer.Deserialize<UploadSignal>(job.Payload);
        if (signal == null)
            throw new InvalidJobException(job, "Can't read payload.");

        var file = new FileInfo(signal.Filepath);
        var hash = await file.HashXx3Async(ct);
    }
}
