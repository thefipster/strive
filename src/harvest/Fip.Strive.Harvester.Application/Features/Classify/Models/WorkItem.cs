using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Harvester.Application.Features.Classify.Models;

public class WorkItem
{
    public required FileSignal Signal { get; set; }
    public FileIndex? Index { get; set; }
    public List<ClassificationResult> Classifications { get; set; } = [];

    public string GetFilePath()
    {
        if (Index is null)
            throw new InvalidOperationException("Cannot get file path without index.");

        if (Index.Files.Count == 0)
            throw new InvalidOperationException("Index contains no files.");

        return Index.Files.First().Key;
    }

    public static WorkItem FromSignal(FileSignal signal)
    {
        return new WorkItem { Signal = signal };
    }

    public TypedSignal ToSignal()
    {
        return new TypedSignal
        {
            ReferenceId = Signal.ReferenceId,
            Filepath = Signal.Filepath,
            Hash = Signal.Hash,
            Source = Index!.Source!.Value,
        };
    }
}
