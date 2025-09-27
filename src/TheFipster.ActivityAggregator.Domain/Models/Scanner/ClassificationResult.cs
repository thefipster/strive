using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Extraction;

namespace TheFipster.ActivityAggregator.Domain.Models.Scanner;

public class ClassificationResult(DataSources source)
{
    public DataSources Source { get; set; } = source;
    public Exception? Exception { get; set; }
    public Classification Classification { get; set; }
    public int Version { get; set; }
}
