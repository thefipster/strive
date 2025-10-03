namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ZipIndex
{
    public required string Hash { get; set; }
    public required string ZipPath { get; set; }
    public required string OutputPath { get; set; }
    public List<string> AlternateFiles { get; set; } = new();
    public int FileCount { get; set; }
    public long UnpackedSize { get; set; }
    public long PackedSize { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
}
