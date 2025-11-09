using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;

public interface IExpandService
{
    Task<string> ExpandAsync(ImportSignal signal, CancellationToken ct = default);
}
