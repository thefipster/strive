using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Import.Models;

namespace Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;

public interface IZipInventory
{
    Task<WorkItem> ImportAsync(UploadSignal signal, CancellationToken ct);
}
