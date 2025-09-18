using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface ITransfomerStage : IStage<ClassificationIndex, TransformIndex> { }
