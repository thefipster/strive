namespace Fipster.TrackMe.Domain.Models.Indexes;

public class Classification(DataSources source, DateTime datetime, DateRanges range)
{
    public DataSources Source => source;
    public DateTime Datetime => datetime;
    public DateRanges Range => range;
}
