using System.IO.Compression;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

namespace Fip.Strive.Core.Application.Features.FileSystem.Services;

public class ZipFileExtractor : IZipExtractor
{
    public void ExtractToDirectory(string zipPath, string destinationPath, bool overwrite)
    {
        ZipFile.ExtractToDirectory(zipPath, destinationPath, overwrite);
    }
}
