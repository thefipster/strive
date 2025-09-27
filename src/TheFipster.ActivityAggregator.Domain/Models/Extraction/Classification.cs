using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Extraction;

public class Classification(DataSources source, DateTime datetime, DateRanges range)
{
    public DataSources Source => source;
    public DateTime Datetime => datetime;
    public DateRanges Range => range;

    public int Version { get; set; }

    public override string ToString()
    {
        return Source.ToString();
    }
}
