using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;

namespace Fip.Strive.Core.Ingestion.Models;

public class GpxPoint : GpsPoint
{
    public DateTime? Timestamp { get; set; }
    public double? Speed { get; set; }
}
