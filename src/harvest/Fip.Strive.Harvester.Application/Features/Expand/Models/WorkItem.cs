using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Harvester.Application.Features.Expand.Models;

public class WorkItem
{
    public Guid Id { get; set; }
    public Guid ReferenceId { get; set; }
    public required string ZipPath { get; set; }
    public string? OutputPath { get; set; }
    public string? Hash { get; set; }

    public static WorkItem FromSignal(ImportSignal signal)
    {
        return new WorkItem
        {
            Id = signal.Id,
            ReferenceId = signal.ReferenceId,
            ZipPath = signal.Filepath,
        };
    }

    public FileIndex ToIndex(string hash)
    {
        Hash = hash;
        return new FileIndex
        {
            Hash = hash,
            ReferenceId = ReferenceId,
            SignalId = Id,
        };
    }

    public FileSignal ToSignal(string filepath)
    {
        return new FileSignal
        {
            Id = Id,
            ReferenceId = ReferenceId,
            Filepath = filepath,
        };
    }
}
