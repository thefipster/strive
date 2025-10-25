using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Harvester.Application.Features.Expand.Models;

public class WorkItem
{
    public required ImportSignal Signal { get; init; }
    public string? OutputPath { get; set; }
    public string? Hash { get; set; }

    public static WorkItem FromSignal(ImportSignal signal)
    {
        return new WorkItem { Signal = signal };
    }

    public FileIndex ToIndex(string hash)
    {
        Hash = hash;
        return new FileIndex
        {
            Hash = Hash,
            ReferenceId = Signal.ReferenceId,
            SignalledAt = Signal.EmittedAt,
            SignalId = Signal.Id,
        };
    }

    public FileSignal ToSignal(string filepath)
    {
        return new FileSignal
        {
            Id = Signal.Id,
            ReferenceId = Signal.ReferenceId,
            Filepath = filepath,
        };
    }
}
