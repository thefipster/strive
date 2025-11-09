using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Indexing.Domain.Models;

public class TypeMeta
{
    public required string Hash { get; set; }
    public required DataSources Source { get; set; }
    public required int Version { get; set; }
}
