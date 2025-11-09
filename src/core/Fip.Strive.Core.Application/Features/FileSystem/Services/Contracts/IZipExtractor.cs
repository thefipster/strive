namespace Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

public interface IZipExtractor
{
    void Unzip(string zipPath, string destinationPath, bool overwrite);
}
