using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Index.Models;

public class DataIndex
{
    public required string Hash { get; set; }
    public required string Filepath { get; set; }
    public required DataKind Kind { get; set; }
    public required DateTime Timestamp { get; set; }
    public required DataSources Source { get; set; }
    public required string SourceFile { get; set; }
    public required Guid ReferenceId { get; set; }
    public required string ParentId { get; set; }

    public required DateTime SignalledAt { get; set; }
    public required Guid SignalId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
