namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ScanIndex(int version, string directory, string filepath, string hash)
{
    public int Version { get; set; } = version;
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public string Directory { get; set; } = directory;
    public string Filepath { get; set; } = filepath;
    public string Hash { get; set; } = hash;
}
