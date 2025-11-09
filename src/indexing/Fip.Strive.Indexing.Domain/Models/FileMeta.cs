namespace Fip.Strive.Indexing.Domain.Models;

public class FileMeta
{
    public required string Hash { get; set; }

    public List<string> Files { get; set; } = [];
    public List<string> ClassificationHashes { get; set; } = [];
}
