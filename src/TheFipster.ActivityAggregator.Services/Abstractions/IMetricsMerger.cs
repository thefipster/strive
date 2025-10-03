using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Merging;

namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface IMetricsMerger
{
    MetricMergeResult Merge(params Dictionary<Parameters, string>[] metrics);
}
