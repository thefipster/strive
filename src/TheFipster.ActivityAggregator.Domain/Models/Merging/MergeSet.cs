using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Merging;

public class MergeSet
{
    // import
    public List<FileExtraction> Extractions { get; set; }

    public object AllMetrics => Extractions.Select(x => x.Attributes);

    //
    public Dictionary<SampleTypes, List<NormalizedSample>> Samples { get; set; }

    public MetricMergeResult MetricsMerge { get; set; }
}
