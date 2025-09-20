namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ScanIndex
{
    public ScanIndex() { }

    public ScanIndex(int version, string directory, string filepath, string hash)
    {
        Version = version;
        Directory = directory;
        Filepath = filepath;
        Hash = hash;
    }

    public int Version { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public string? Directory { get; set; }
    public string? Filepath { get; set; }
    public string? Hash { get; set; }
}
