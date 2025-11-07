using Fip.Strive.Harvester.Application.Features.Assimilate.Models;
using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;

public interface IAssimilationService
{
    Task<WorkItem> ExtractFileAsync(TypedSignal signal, CancellationToken ct);
}
