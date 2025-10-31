using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Ingestion.Models;

public class DateEntry
{
    private DateTime _timestamp;

    public DateTime Timestamp
    {
        get => _timestamp;
        set
        {
            _timestamp = value;

            Date = _timestamp.Date;
            Year = _timestamp.Year;
            Month = _timestamp.Month;
            Day = _timestamp.Day;
        }
    }

    public DateTime Date { get; set; }
    public DataKind Kind { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
}
