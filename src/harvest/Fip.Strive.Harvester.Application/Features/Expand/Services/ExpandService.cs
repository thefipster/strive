using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services;

public class ExpandService(
    IOptions<ExpandConfig> config,
    IZipService service,
    IDirectoryService directory,
    IFileEmitter enumerator
) : IExpandService
{
    private readonly string _rootPath = config.Value.Path;
    private readonly bool _overwrite = config.Value.Overwrite;

    public Task<string> ExpandAsync(ImportSignal signal, CancellationToken ct = default)
    {
        var filepath = signal.Filepath;
        var filename = Path.GetFileNameWithoutExtension(filepath);
        var outputPath = Path.Combine(_rootPath, filename);

        directory.Create(outputPath);
        service.Unzip(filepath, outputPath, _overwrite);
        enumerator.ScanFolderAsync(outputPath, signal, ct);

        return Task.FromResult(outputPath);
    }
}
