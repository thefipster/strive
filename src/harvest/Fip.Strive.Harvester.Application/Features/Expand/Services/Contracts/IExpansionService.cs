using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;

public interface IExpansionService
{
    Task UnpackZipFileAsync(ImportSignal signal, CancellationToken ct);
}
