using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;

public interface IImportService
{
    Task<string?> MoveZipAsync(UploadSignal signal, CancellationToken ct = default);
}
