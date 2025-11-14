namespace Fip.Strive.Harvester.Domain.Indexes;

public class FileIndexV2
{
    public required string Filepath { get; set; }
    public required string ParentFilepath { get; set; }
    public required string Hash { get; set; }
    public required string ClassificationHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static ZipIndexV2 Create(string filepath, string signalHash)
    {
        return new ZipIndexV2 { Filepath = filepath, Hash = signalHash };
    }
}
