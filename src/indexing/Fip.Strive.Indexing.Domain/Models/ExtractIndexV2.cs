using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Indexing.Domain.Models;

public class ExtractIndexV2
{
    public required string Hash { get; set; }
    public required string Filepath { get; set; }
    public required string ParentHash { get; set; }
    public required DataSources Source { get; set; }
    public required string ParentFilepath { get; set; }
    public required DateTime Timestamp { get; set; }
    public required DataKind Kind { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
