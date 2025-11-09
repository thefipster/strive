using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

namespace Fip.Strive.Core.Application.Features.FileSystem.Services;

[ExcludeFromCodeCoverage]
public class ZipFileExtractor : IZipExtractor
{
    public void Unzip(string zipPath, string destinationPath, bool overwrite)
    {
        ZipFile.ExtractToDirectory(zipPath, destinationPath, overwrite);
    }
}
