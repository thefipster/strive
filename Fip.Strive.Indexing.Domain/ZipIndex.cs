namespace Fip.Strive.Indexing.Domain;

public class ZipIndex
{
    public required string Hash { get; set; }
    public Dictionary<string, DateTime> Files { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime SignalledAt { get; set; }
    public Guid SignalId { get; set; }
    public Guid ReferenceId { get; set; }

    public void AddFile(string filename) => Files.Add(filename, DateTime.UtcNow);
}
