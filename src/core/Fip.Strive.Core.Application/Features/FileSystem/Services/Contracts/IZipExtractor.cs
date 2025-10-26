namespace Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

public interface IZipExtractor
{
    void ExtractToDirectory(string zipPath, string destinationPath, bool overwrite);
}
