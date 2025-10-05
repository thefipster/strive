using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class BatchIndex
{
    public DateTime Timestamp { get; set; }
    public DataKind Kind { get; set; }
    public List<string> Parameters { get; set; } = [];
    public required string Filepath { get; set; }
    public required string Hash { get; set; }
    public List<DataSources> Sources { get; set; } = [];

    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public int Metrics { get; set; }
    public int Series { get; set; }
    public int Tracks { get; set; }
    public int Pulses { get; set; }
    public int Events { get; set; }
    public Dictionary<string, string> Assimilations { get; set; }
}
