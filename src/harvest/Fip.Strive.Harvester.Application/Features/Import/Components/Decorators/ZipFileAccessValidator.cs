using Fip.Strive.Harvester.Application.Features.Import.Components.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Import.Components.Decorators;

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

    public FileStream Open(string importPath)
    {
        if (string.IsNullOrWhiteSpace(importPath))
            throw new ArgumentException("Import path cannot be null or empty.", nameof(importPath));

        if (!File.Exists(importPath))
            throw new FileNotFoundException("The specified file does not exist.", importPath);

        return component.Open(importPath);
    }
}