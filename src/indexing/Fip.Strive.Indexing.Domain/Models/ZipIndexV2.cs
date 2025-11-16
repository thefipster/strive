namespace Fip.Strive.Indexing.Domain.Models;

public class ZipIndexV2
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
