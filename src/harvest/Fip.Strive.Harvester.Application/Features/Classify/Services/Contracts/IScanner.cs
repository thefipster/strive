using Fip.Strive.Harvester.Application.Features.Classify.Models;
using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Application.Features.Classify.Services.Contracts;

public interface IScanner
{
    Task<WorkItem> ClassifyAsync(FileSignal signal, CancellationToken ct);
}
