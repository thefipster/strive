using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Harvester.Application.Features.Classify.Services.Contracts;

public interface IScanner
{
    Task ClassifyAsync(FileSignal signal, CancellationToken ct);
}
