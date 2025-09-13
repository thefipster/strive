using Fipster.TrackMe.Pipeline.Models;

namespace Fipster.TrackMe.Pipeline.Abstractions;

public interface IPipeline
{
    Task ExecuteAsync(CancellationToken token);

    event EventHandler<ProgressReportEventArgs>? ReportProgress;
    event EventHandler<ErrorReportEventArgs>? ReportError;
}
