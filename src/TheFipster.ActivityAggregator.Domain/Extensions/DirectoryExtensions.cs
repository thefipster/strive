using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Domain.Extensions;

public static class DirectoryExtensions
{
    public static DirectoryStats GetFileCountAndSize(this DirectoryInfo directory)
    {
        if (!directory.Exists)
            throw new DirectoryNotFoundException($"Directory not found: {directory.FullName}");

        int fileCount = 0;
        long totalSize = 0;

        void ProcessDirectory(string path)
        {
            var files = Directory.GetFiles(path);
            fileCount += files.Length;

            foreach (var file in files)
            {
                totalSize += new FileInfo(file).Length;
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                ProcessDirectory(dir);
            }
        }

        ProcessDirectory(directory.FullName);

        return new DirectoryStats { FileCount = fileCount, Size = totalSize };
    }
}
