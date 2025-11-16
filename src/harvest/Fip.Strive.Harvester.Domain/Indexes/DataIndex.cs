using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Harvester.Domain.Indexes;

public class DataIndex
{
    public required string Hash { get; set; }
    public required string Filepath { get; set; }
    public DataSources Source { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
