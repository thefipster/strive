using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class AssimilateIndex
{
    public required string FileHash { get; set; }
    public required string Hash { get; set; }
    public required string Path { get; set; }
    public long Size { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DataSources Source { get; set; }
    public DateTime Timestamp { get; set; }
    public DataKind Kind { get; set; }
    public List<string> Metrics { get; set; } = [];
}
