using Fip.Strive.Core.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Models;

public class GpxPoint : GpsPoint
{
    public DateTime? Timestamp { get; set; }
    public double? Speed { get; set; }
}
