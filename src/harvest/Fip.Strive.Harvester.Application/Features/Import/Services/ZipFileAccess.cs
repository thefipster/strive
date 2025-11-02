using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Import.Services;

public class ZipFileAccess(
    IOptions<ImportConfig> config,
    IDirectoryService directory,
    IFileService fileService
) : IZipFileAccess
{
    private readonly string _rootPath = config.Value.Path;

    public string Import(string uploadPath)
    {
        directory.CreateDirectory(_rootPath);

        string fileName = Path.GetFileName(uploadPath);
        string destinationPath = Path.Combine(_rootPath, fileName);

        fileService.Copy(uploadPath, destinationPath, overwrite: false);
        fileService.Delete(uploadPath);

        return destinationPath;
    }
}
