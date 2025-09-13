using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Models;

namespace TheFipster.ActivityAggregator.Pipeline;

public class Pipeline(IScannerService scannerService) : IPipeline
{
    public event EventHandler<ProgressReportEventArgs>? ReportProgress;
    public event EventHandler<ErrorReportEventArgs>? ReportError;

    public async Task ExecuteAsync(CancellationToken token)
    {
        scannerService.ReportProgress += ScannerServiceOnReportProgress;
        scannerService.ReportError += ScannerServiceOnReportError;

        await scannerService.ExecuteAsync(token);
    }

    private void ScannerServiceOnReportError(object? sender, ErrorReportEventArgs e)
    {
        ReportError?.Invoke(this, e);
    }

    private void ScannerServiceOnReportProgress(object? sender, ProgressReportEventArgs e)
    {
        ReportProgress?.Invoke(this, e);
    }
}
