using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Domain.Models;

public class ClassificationResult(DataSources source)
{
    public DataSources Source { get; set; } = source;
    public Exception? Exception { get; set; }
    public Classification? Classification { get; set; }

    public int Version { get; set; }
}
