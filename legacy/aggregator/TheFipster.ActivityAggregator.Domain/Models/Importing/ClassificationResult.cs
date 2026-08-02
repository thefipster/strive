using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Importing;

public class ClassificationResult(DataSources source)
{
    public DataSources Source { get; set; } = source;
    public Exception? Exception { get; set; }
    public Classification? Classification { get; set; }
    public int Version { get; set; }
}
