using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface ITransfomerStage : IStage
{
    void Enqueue(ClassificationIndex classification);
    event EventHandler<ResultReportEventArgs<TransformIndex>>? ReportResult;
}
