using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Models;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;

public interface IFileHashGate
{
    Task<FileIndex> CheckFileAsync(WorkItem work, string filepath, CancellationToken ct);
}
