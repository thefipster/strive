using Fipster.TrackMe.Pipeline.Models;

namespace Fipster.TrackMe.Pipeline.Abstractions;

public interface IScannerService : IStage
{
    event EventHandler<ImportReadyEventArgs>? ReportImport;
}
