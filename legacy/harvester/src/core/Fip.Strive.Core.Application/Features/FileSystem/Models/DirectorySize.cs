namespace Fip.Strive.Core.Application.Features.FileSystem.Models;

public class DirectorySize
{
    public required string OutputPath { get; set; }
    public int FileCount { get; set; }
    public long TotalSize { get; set; }

    public static DirectorySize New(string outputPath, int fileCount, long totalSize)
    {
        return new DirectorySize
        {
            OutputPath = outputPath,
            FileCount = fileCount,
            TotalSize = totalSize,
        };
    }
}
