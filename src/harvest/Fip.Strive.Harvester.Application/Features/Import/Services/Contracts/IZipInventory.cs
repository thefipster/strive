using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;

public interface IZipInventory
{
    Task<ZipIndex> ImportAsync(UploadSignal signal, CancellationToken ct);
}
