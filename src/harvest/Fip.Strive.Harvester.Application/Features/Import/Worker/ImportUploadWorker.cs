using System.CodeDom.Compiler;
using System.Text.Json;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Extensions;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Enums;
using Fip.Strive.Harvester.Application.Core.Queue.Exceptions;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Features.Import.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Models;
using Fip.Strive.Harvester.Application.Features.Upload.Signals;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Import.Worker;

public class ImportUploadWorker(IZipIndexer indexer, IOptions<ImportConfig> config)
    : ISignalQueueWorker
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
        var filename = file.Name;
        var hash = await file.HashXx3Async(ct);

        var index = indexer.Get(hash);

        if (index != null)
        {
            if (index.Files.ContainsKey(filename))
            {
                File.Delete(signal.Filepath);
                return;
            }
        }
        else
        {
            var newPath = Path.Combine(config.Value.Path, filename);
            if (File.Exists(newPath))
                throw new ImportException(filename, "File already exists.");

            File.Copy(signal.Filepath, newPath);

            index = new ZipIndex
            {
                Hash = hash,
                UploadedAt = signal.EmittedAt,
                SignalId = signal.Id,
            };
        }

        index.AddFile(filename);
        indexer.Set(index);
    }
}
