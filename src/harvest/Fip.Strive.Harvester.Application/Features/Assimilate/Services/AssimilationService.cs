using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Assimilate.Models;
using Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services;

public class AssimilationService : IAssimilationService
{
    public Task<WorkItem> ExtractFileAsync(TypedSignal signal, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
