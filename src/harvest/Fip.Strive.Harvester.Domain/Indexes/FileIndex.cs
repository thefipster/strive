namespace Fip.Strive.Harvester.Domain.Indexes;

public class FileIndex
{
    public required string Filepath { get; set; }
    public required string ParentFilepath { get; set; }
    public required string Hash { get; set; }
    public required string ClassificationHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
