using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ExtractorIndex
{
    public required string ZipHash { get; set; }
    public required string FileHash { get; set; }
    public required string ValueHash { get; set; }
    public required string Path { get; set; }
    public long Size { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DataSources? Source { get; set; }
    public DateTime? Timestamp { get; set; }
    public DateRanges? Range { get; set; }
    public Dictionary<string, string> ExtractedFiles { get; set; } = new();
}
