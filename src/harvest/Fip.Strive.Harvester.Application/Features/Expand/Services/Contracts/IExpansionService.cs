using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Expand.Models;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;

public interface IExpansionService
{
    Task<WorkItem> UnpackZipFileAsync(ImportSignal signal, CancellationToken ct);
}
