using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Harvester.Application.Features.Import.Models;

public class WorkItem
{
    public required UploadSignal Signal { get; init; }
    public string? ImportedPath { get; set; }
    public string? Hash { get; set; }
    public string Filename => Path.GetFileName(Signal.Filepath);
    public ZipIndex? Index { get; set; }
    public bool Skip { get; set; }

    public static WorkItem FromSignal(UploadSignal signal)
    {
        return new WorkItem { Signal = signal };
    }

    public ImportSignal ToSignal(string filepath)
    {
        return new ImportSignal { ReferenceId = Signal.ReferenceId, Filepath = filepath };
    }

    public ZipIndex ToIndex()
    {
        if (string.IsNullOrWhiteSpace(Hash))
            throw new InvalidOperationException(
                "Cannot create ZipIndex when hash is null or empty."
            );

        var index = new ZipIndex
        {
            SignalId = Signal.Id,
            ReferenceId = Signal.ReferenceId,
            SignalledAt = Signal.EmittedAt,
            Hash = Hash,
        };

        index.AddFile(Filename);

        return index;
    }
}
