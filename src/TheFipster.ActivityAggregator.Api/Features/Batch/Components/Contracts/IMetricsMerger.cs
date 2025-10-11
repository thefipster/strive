using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components.Contracts;

public interface IMetricsMerger
{
    MetricMergeResult Merge(params Dictionary<Parameters, string>[] metrics);
}
