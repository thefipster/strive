using Fip.Strive.Harvester.Application.Features.Import.Components.Contracts;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Import.Components;

public class ZipFileAccess(IOptions<ImportConfig> config) : IZipFileAccess
{
    private readonly string _rootPath = config.Value.Path;

    public string Import(string uploadPath)
    {
        Directory.CreateDirectory(_rootPath);

        string fileName = Path.GetFileName(uploadPath);
        string destinationPath = Path.Combine(_rootPath, fileName);

        File.Copy(uploadPath, destinationPath, overwrite: false);
        File.Delete(uploadPath);

        return destinationPath;
    }

    public FileStream Open(string importPath) =>
        new FileStream(
            importPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true
        );
}
