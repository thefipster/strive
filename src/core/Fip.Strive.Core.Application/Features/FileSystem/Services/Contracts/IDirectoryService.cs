using Fip.Strive.Core.Application.Features.FileSystem.Models;

namespace Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

public interface IDirectoryService
{
    void CreateDirectory(string path);

    IEnumerable<string> EnumerateAllFiles(string path);

    DirectorySize GetFileCountAndSize(string path);
}
