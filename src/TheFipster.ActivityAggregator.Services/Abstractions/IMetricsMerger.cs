using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;

namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface IMetricsMerger
{
    MetricMergeResult Merge(params Dictionary<Parameters, string>[] metrics);
}
