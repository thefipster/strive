using Fip.Strive.Core.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Domain.Models;

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
