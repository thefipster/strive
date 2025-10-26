using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Ingestion.Models;

public class ClassificationResult(DataSources source)
{
    public DataSources Source { get; set; } = source;
    public Exception? Exception { get; set; }
    public Classification? Classification { get; set; }
    public int Version { get; set; }
}
