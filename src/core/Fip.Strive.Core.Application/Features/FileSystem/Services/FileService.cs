using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

namespace Fip.Strive.Core.Application.Features.FileSystem.Services;

[ExcludeFromCodeCoverage]
public class FileService : IFileService
{
    public void Copy(string uploadPath, string destinationPath, bool overwrite = false) =>
        File.Copy(uploadPath, destinationPath, overwrite);

    public void Delete(string uploadPath) => File.Delete(uploadPath);
}
