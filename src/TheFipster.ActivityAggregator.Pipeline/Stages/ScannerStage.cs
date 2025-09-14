using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;

namespace TheFipster.ActivityAggregator.Pipeline.Stages;

public class ScannerStage(IOptions<ScannerConfig> scannerConfig, PipelineState state)
    : IScannerStage
{
    public const int Version = 1;
    public const string Name = "scanner";

    private readonly ScannerConfig config = scannerConfig.Value;
    private CancellationToken token;
    private readonly ProgressCounters counters = new();

    public event EventHandler<ProgressReportEventArgs>? ReportProgress;
    public event EventHandler<ErrorReportEventArgs>? ReportError;
    public event EventHandler<ResultReportEventArgs<ImportIndex>>? ReportResult;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        token = cancellationToken;
        var tasks = new List<Task> { CreateScanTask(), CreateReportTask() };
        await Task.WhenAll(tasks);
    }

    #region Scanner

    private Task CreateScanTask() =>
        Task.Run(
            () =>
            {
                Parallel.ForEach(
                    config.Include,
                    new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = token },
                    ScanImportDirectory
                );
                state.FinishedStages.Add(Name);
            },
            token
        );

    private void ScanImportDirectory(string directoryPath)
    {
        Parallel.ForEachAsync(
            Directory
                .EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories)
                .Select(filepath => new FileInfo(filepath)),
            new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = token },
            async (file, _) =>
            {
                counters.In.Increment();
                await ProcessFileAsync(file);
                counters.Done.Increment();
            }
        );
    }

    private async Task ProcessFileAsync(FileInfo file)
    {
        var filepath = file.FullName;
        var hash = await file.HashXx3Async(token);
        var import = new ImportIndex(Version, filepath, hash);

        ReportResult?.Invoke(this, new(import));
        counters.Out.Increment();
    }

    #endregion

    #region Progress

    private Task CreateReportTask()
    {
        var reportTask = Task.Run(
            async () =>
            {
                while (!token.IsCancellationRequested && !state.FinishedStages.Contains(Name))
                    await EmitProgress(1000);
            },
            token
        );

        return reportTask;
    }

    private async Task EmitProgress(int delayInMs)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(delayInMs), token);
        ReportProgress?.Invoke(
            this,
            new ProgressReportEventArgs(
                Name,
                0,
                counters.In.Value,
                counters.Done.Value,
                counters.Skip.Value,
                counters.Out.Value
            )
        );
    }

    #endregion
}
