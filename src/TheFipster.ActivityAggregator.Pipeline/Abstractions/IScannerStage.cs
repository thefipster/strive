using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface IScannerStage : IStage
{
    event EventHandler<ResultReportEventArgs<ImportIndex>>? ReportResult;
}
