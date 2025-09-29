using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ConvergeIndex
{
    public required string ImportHash { get; set; }
    public required string SourceHash { get; set; }
    public required string ValueHash { get; set; }
    public required string FileHash { get; set; }
    public required string Filepath { get; set; }
    public DataSources Source { get; set; }
    public DateTime Timestamp { get; set; }
    public DataKind Kind { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<DateTime, ConvergeActions> Actions { get; set; } = new();
}
