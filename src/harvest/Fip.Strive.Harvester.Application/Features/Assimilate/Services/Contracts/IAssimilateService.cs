using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;

public interface IAssimilateService
{
    Task ExtractFileAsync(TypedSignal signal, CancellationToken ct);
}
