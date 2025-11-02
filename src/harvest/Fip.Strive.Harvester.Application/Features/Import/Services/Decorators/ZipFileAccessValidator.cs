using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Import.Services.Decorators;

public class ZipFileAccessValidator(IZipFileAccess component) : IZipFileAccess
{
    public string Import(string uploadPath)
    {
        if (string.IsNullOrWhiteSpace(uploadPath))
            throw new ArgumentException("Zip path cannot be null or empty.", nameof(uploadPath));

        if (!File.Exists(uploadPath))
            throw new FileNotFoundException("The specified zip file does not exist.", uploadPath);

        return component.Import(uploadPath);
    }
}
