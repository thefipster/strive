using TheFipster.ActivityAggregator.Pipeline.Models;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface IScannerService : IStage
{
    event EventHandler<ImportReadyEventArgs>? ReportImport;
}
