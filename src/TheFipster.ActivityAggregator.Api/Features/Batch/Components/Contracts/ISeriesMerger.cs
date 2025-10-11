using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components.Contracts;

public interface ISeriesMerger
{
    NormalizedResult Normalize(Dictionary<Parameters, List<string>> series);
}
