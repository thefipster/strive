using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Ingestion.Models;

public class Classification(DataSources source, DateTime datetime, DateRanges range)
{
    public DataSources Source => source;
    public int Version { get; set; }

    public DateTime Datetime => datetime;
    public DateRanges Range => range;

    public override string ToString() => $"{Source}:v{Version}";
}
