namespace Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

public interface IFileService
{
    void Copy(string sourceFilepath, string destinationFilepath, bool overwrite = false);
    void Delete(string filepath);
}
