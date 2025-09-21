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
        SetId();
    }

    public Inventory(int year, int month, int day)
    {
        Year = year;
        Month = month;
        Day = day;
        SetId();
    }

    private void SetId() => Id = Year * 10000 + Month * 100 + Day;

    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }

    public HashSet<Parameters> Metrics { get; set; } = [];
    public HashSet<SampleTypes> Series { get; set; } = [];
    public HashSet<EventTypes> Events { get; set; } = [];
}
