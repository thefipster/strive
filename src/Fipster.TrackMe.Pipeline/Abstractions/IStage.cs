using Fipster.TrackMe.Pipeline.Models;

namespace Fipster.TrackMe.Pipeline.Abstractions;

public interface IStage
{
    Task ExecuteAsync(CancellationToken token = default);

    event EventHandler<ProgressReportEventArgs>? ReportProgress;
    event EventHandler<ErrorReportEventArgs>? ReportError;
}
