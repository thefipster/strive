using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

namespace Fip.Strive.Core.Application.Features.FileSystem.Services;

[ExcludeFromCodeCoverage]
public class FileService : IFileService
{
    public void Copy(string sourceFilepath, string destinationFilepath, bool overwrite = false)
    {
        var directory = Path.GetDirectoryName(destinationFilepath);

        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.Copy(sourceFilepath, destinationFilepath, overwrite);
    }

    public void Delete(string filepath)
    {
        if (File.Exists(filepath))
            File.Delete(filepath);
    }
}
