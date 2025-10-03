using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class FileIndex
{
    public required string ZipHash { get; set; }
    public required string Hash { get; set; }
    public required string Path { get; set; }
    public List<string> AlternateFiles { get; set; } = new();
    public long Size { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DataSources? Source { get; set; }
    public DateTime? Timestamp { get; set; }
    public DateRanges? Range { get; set; }
}
