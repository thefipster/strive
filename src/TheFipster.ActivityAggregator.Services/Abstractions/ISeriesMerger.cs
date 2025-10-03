using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Merging;

namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface ISeriesMerger
{
    NormalizedResult Normalize(Dictionary<Parameters, List<string>> series);
}
