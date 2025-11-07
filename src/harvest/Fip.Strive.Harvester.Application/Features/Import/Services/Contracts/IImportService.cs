using Fip.Strive.Harvester.Application.Features.Import.Models;
using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;

public interface IImportService
{
    Task<WorkItem> ProcessUploadAsync(UploadSignal signal, CancellationToken ct);
}
