using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Import.Services;

public class ImportService(
    IOptions<ImportConfig> config,
    IDirectoryService directory,
    IFileService fileService
) : IImportService
{
    private readonly string _rootPath = config.Value.Path;
    private readonly bool _overwrite = config.Value.Overwrite;

    public Task<string?> MoveZipAsync(UploadSignal signal, CancellationToken ct = default)
    {
        directory.Create(_rootPath);

        string fileName = Path.GetFileName(signal.Filepath);
        string destinationPath = Path.Combine(_rootPath, fileName);

        fileService.Copy(signal.Filepath, destinationPath, overwrite: _overwrite);
        fileService.Delete(signal.Filepath);

        return Task.FromResult(destinationPath)!;
    }
}
