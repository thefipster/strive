namespace Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

public interface IZipService
{
    void Unzip(string zipPath, string destinationPath, bool overwrite = false);
}
