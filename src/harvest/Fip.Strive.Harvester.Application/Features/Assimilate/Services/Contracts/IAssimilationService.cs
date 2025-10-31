using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Assimilate.Models;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;

public interface IAssimilationService
{
    Task<WorkItem> ExtractFileAsync(TypedSignal signal, CancellationToken ct);
}
