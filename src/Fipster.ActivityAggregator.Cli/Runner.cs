using Fipster.TrackMe.Pipeline.Abstractions;
using Fipster.TrackMe.Pipeline.Models;

namespace Fipster.ActivityAggregator.Cli;

public class Runner(IPipeline pipeline)
{
    public async Task ExecuteAsync(CancellationToken token = default)
    {
        pipeline.ReportProgress += PipelineOnReportProgress;
        pipeline.ReportError += PipelineOnReportError;

        await pipeline.ExecuteAsync(token);
    }

    private void PipelineOnReportError(object? sender, ErrorReportEventArgs e)
    {
        Console.Error.WriteLine(e.Message);
    }

    private void PipelineOnReportProgress(object? sender, ProgressReportEventArgs e)
    {
        Console.WriteLine(
            $"{e.Stage + ":", -16}{e.Processed, 5}:{e.Errors, 5} / {e.Total, 5} - {Math.Round((e.Processed + e.Errors) * 100.0 / e.Total, 0)}%"
        );
    }
}
