using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Classify.Models;

namespace Fip.Strive.Harvester.Application.Features.Classify.Services.Contracts;

public interface IScanner
{
    Task<WorkItem> ClassifyAsync(FileSignal signal, CancellationToken ct);
}
