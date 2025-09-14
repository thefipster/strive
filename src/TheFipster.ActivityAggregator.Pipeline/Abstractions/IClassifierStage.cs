using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface IClassifierStage : IStage
{
    event EventHandler<ResultReportEventArgs<ClassificationIndex>>? ReportResult;
    void Enqueue(ImportIndex import);
}
