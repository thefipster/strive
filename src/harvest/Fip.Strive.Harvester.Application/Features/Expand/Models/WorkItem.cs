using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Harvester.Application.Features.Expand.Models;

public class WorkItem
{
    public required ImportSignal Signal { get; init; }
    public string? OutputPath { get; set; }
    public FileIndex? Index { get; set; }

    public static WorkItem FromSignal(ImportSignal signal)
    {
        return new WorkItem { Signal = signal };
    }

    public FileIndex ToIndex(string hash)
    {
        Index = new FileIndex
        {
            Hash = hash,
            ReferenceId = Signal.ReferenceId,
            SignalledAt = Signal.EmittedAt,
            SignalId = Signal.Id,
            ParentId = Signal.Hash,
        };

        return Index;
    }

    public FileSignal ToSignal(string filepath)
    {
        return new FileSignal
        {
            ReferenceId = Signal.ReferenceId,
            Hash =
                Index?.Hash
                ?? throw new InvalidOperationException(
                    $"Cannot create FileSignal without Index for file {filepath}."
                ),
            Filepath = filepath,
        };
    }
}
