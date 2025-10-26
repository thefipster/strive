namespace Fip.Strive.Core.Application.Features.FileSystem.Models;

public class DirectorySize
{
    public required string OutputPath { get; set; }
    public int FileCount { get; set; }
    public long Size { get; set; }
}
