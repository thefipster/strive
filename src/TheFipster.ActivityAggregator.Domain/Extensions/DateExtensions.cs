namespace TheFipster.ActivityAggregator.Domain.Extensions
{
    public static class DateExtensions
    {
        public static string GetPath(this DateRanges type, DateTime date)
        {
            return type switch
            {
                DateRanges.Time => $"{date:yyyy}\\{date:MM}\\{date:dd}\\{date:HH_mm}",
                DateRanges.Day => $"{date:yyyy}\\{date:MM}\\{date:dd}",
                DateRanges.Month => $"{date:yyyy}\\{date:MM}",
                DateRanges.Year => $"{date:yyyy}",
                _ => string.Empty,
            };
        }

        public static string ToRangeString(this DateTime date, DateRanges range)
        {
            return range switch
            {
                DateRanges.Time => $"{date:yyyy}_{date:MM}_{date:dd}_{date:HH_mm}",
                DateRanges.Day => $"{date:yyyy}_{date:MM}_{date:dd}",
                DateRanges.Month => $"{date:yyyy}_{date:MM}",
                DateRanges.Year => $"{date:yyyy}",
                _ => string.Empty,
            };
        }
    }
}
