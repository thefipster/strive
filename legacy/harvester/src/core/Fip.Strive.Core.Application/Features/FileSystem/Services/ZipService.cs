using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

namespace Fip.Strive.Core.Application.Features.FileSystem.Services;

[ExcludeFromCodeCoverage]
public class ZipService : IZipService
{
    public void Unzip(string zipPath, string destinationPath, bool overwrite = false)
    {
        ZipFile.ExtractToDirectory(zipPath, destinationPath, overwrite);
    }
}
