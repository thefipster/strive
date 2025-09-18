using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface IClassifierStage : IStage<ScanIndex, ClassificationIndex> { }
