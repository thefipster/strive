using Fip.Strive.Harvester.Application.Features.Classify.Models;
using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Application.Features.Classify.Services.Contracts;

public interface IClassifyService
{
    Task ScanFile(FileSignal signal, CancellationToken ct);
}
