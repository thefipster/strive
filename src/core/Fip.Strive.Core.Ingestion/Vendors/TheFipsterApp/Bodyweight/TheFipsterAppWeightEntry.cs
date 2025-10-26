namespace Fip.Strive.Core.Ingestion.Vendors.TheFipsterApp.Bodyweight;

public class TheFipsterAppWeightEntry
{
    public double Weight { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
