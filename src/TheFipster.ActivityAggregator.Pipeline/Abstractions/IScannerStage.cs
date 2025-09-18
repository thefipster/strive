using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface IScannerStage : IStage<string, ScanIndex> { }
