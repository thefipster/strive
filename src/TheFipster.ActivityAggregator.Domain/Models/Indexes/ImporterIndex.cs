using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ImporterIndex
{
    public required string Hash { get; set; }
    public required string Output { get; set; }

    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public List<string> Files { get; set; } = new();
    public Dictionary<DateTime, ImporterActions> Actions { get; set; } = new();
    public int Count { get; set; }
    public long UnpackedSize { get; set; }
    public long PackedSize { get; set; }
}
