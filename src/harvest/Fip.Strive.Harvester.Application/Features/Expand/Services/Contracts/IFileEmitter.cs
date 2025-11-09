using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;

public interface IFileEmitter
{
    Task ScanFolderAsync(string path, ImportSignal signal, CancellationToken ct);
}
