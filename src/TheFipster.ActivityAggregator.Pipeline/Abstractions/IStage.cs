using TheFipster.ActivityAggregator.Pipeline.Models;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface IStage
{
    Task ExecuteAsync(CancellationToken token = default);

    event EventHandler<ProgressReportEventArgs>? ReportProgress;
    event EventHandler<ErrorReportEventArgs>? ReportError;
}
