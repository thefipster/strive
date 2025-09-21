using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface IBundlerStage : IStage<string, BundleIndex> { }
