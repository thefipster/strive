using TheFipster.ActivityAggregator.Pipeline.Models;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface IPipeline
{
    Task ExecuteAsync(CancellationToken token);

    event EventHandler<ProgressReportEventArgs>? ReportProgress;
    event EventHandler<ErrorReportEventArgs>? ReportError;
}
