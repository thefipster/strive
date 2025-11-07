using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Fip.Strive.Indexing.Domain;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;

public interface IFileHashGate
{
    Task<FileIndex> CheckFileAsync(WorkItem work, string filepath, CancellationToken ct);
}
