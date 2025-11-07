using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Domain.Models;

public class Classification(DataSources source, DateTime datetime, DateRanges range)
{
    public DataSources Source => source;
    public int Version { get; set; }

    public DateTime Datetime => datetime;
    public DateRanges Range => range;

    public override string ToString() => $"{Source}:v{Version}";
}
