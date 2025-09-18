using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface IUnifierStage : IStage<TransformIndex, UnifiedRecord> { }
