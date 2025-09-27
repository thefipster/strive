using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface IMerger
{
    MetricMergeResult Merge(params Dictionary<Parameters, string>[] metrics);
    MergedRecord Combine(BundleIndex extractions);
}
