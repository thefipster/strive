using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models;

public class Inventory
{
    public Inventory() { }

    public Inventory(DateTime timestamp)
    {
        Year = timestamp.Year;
        Month = timestamp.Month;
        Day = timestamp.Day;
        Hour = timestamp.Hour;
        Minute = timestamp.Minute;

        SetId();
    }

    public Inventory(int year, int month, int day)
    {
        Year = year;
        Month = month;
        Day = day;

        SetId();
    }

    public Inventory(int year, int month, int day, int hour, int minute)
        : this(year, month, day)
    {
        Hour = hour;
        Minute = minute;

        SetId();
    }

    private void SetId() => Id = $"{Year:4}{Month:2}{Day:2}{Hour:2}{Minute:2}";

    public string Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }

    public HashSet<Parameters> Metrics { get; set; } = [];
    public HashSet<SampleTypes> Series { get; set; } = [];
    public HashSet<EventTypes> Events { get; set; } = [];
}
