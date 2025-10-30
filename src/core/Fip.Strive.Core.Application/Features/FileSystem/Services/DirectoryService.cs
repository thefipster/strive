using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application.Features.FileSystem.Models;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

namespace Fip.Strive.Core.Application.Features.FileSystem.Services;

[ExcludeFromCodeCoverage]
public class DirectoryService : IDirectoryService
{
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public IEnumerable<string> EnumerateAllFiles(string path) =>
        Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);

    public DirectorySize GetFileCountAndSize(string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        int fileCount = 0;
        long totalSize = 0;

        void ProcessDirectory(string innerPath)
        {
            var files = Directory.GetFiles(innerPath);
            fileCount += files.Length;

            foreach (var file in files)
                totalSize += new FileInfo(file).Length;

            foreach (var dir in Directory.GetDirectories(innerPath))
                ProcessDirectory(dir);
        }

        ProcessDirectory(path);

        return DirectorySize.New(path, fileCount, totalSize);
    }
}
