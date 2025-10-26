namespace Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

public interface IFileService
{
    void Copy(string uploadPath, string destinationPath, bool overwrite = false);
    void Delete(string uploadPath);
}
