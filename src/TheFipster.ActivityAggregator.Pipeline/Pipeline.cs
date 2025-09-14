using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Models;

namespace TheFipster.ActivityAggregator.Pipeline;

public class Pipeline(
    IScannerStage scanner,
    IClassifierStage classifier,
    ITransfomerStage transformer
) : IPipeline
{
    public event EventHandler<ProgressReportEventArgs>? ReportProgress;
    public event EventHandler<ErrorReportEventArgs>? ReportError;

    public async Task ExecuteAsync(CancellationToken token)
    {
        scanner.ReportProgress += EmitProgress;
        scanner.ReportError += EmitError;
        scanner.ReportResult += (_, args) => classifier.Enqueue(args.Result);

        classifier.ReportProgress += EmitProgress;
        classifier.ReportError += EmitError;
        classifier.ReportResult += (_, args) => transformer.Enqueue(args.Result);

        transformer.ReportProgress += EmitProgress;
        transformer.ReportError += EmitError;

        await Task.WhenAll(
            scanner.ExecuteAsync(token),
            classifier.ExecuteAsync(token),
            transformer.ExecuteAsync(token)
        );
    }

    private void EmitError(object? sender, ErrorReportEventArgs e)
    {
        ReportError?.Invoke(this, e);
    }

    private void EmitProgress(object? sender, ProgressReportEventArgs e)
    {
        ReportProgress?.Invoke(this, e);
    }
}
