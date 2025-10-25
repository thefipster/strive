namespace Fip.Strive.Harvester.Application.Features.Expand.Models;

public class DirectorySize
{
    public required string OutputPath { get; set; }
    public int FileCount { get; set; }
    public long Size { get; set; }
}
