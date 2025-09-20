using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;
using TheFipster.ActivityAggregator.Pipeline.Pipelines;
using TheFipster.ActivityAggregator.Storage.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Pipeline.Stages;

public class BundlerStage(
    PipelineState<MergerPipeline> state,
    IOptions<BundlerConfig> config,
    IIndexer<BundleIndex> indexer,
    ILogger<BundlerStage> logger
) : Stage<string, BundleIndex>, IBundlerStage
{
    public int Version => 1;
    public int Order => 10;
    public event EventHandler<ResultReportEventArgs<BundleIndex>>? ReportResult;

    public async Task ExecuteAsync(CancellationToken ct)
    {
        await CreateBundleTask(ct);
        state.FinishedStages.Add(GetType().Name);
    }

    private Task CreateBundleTask(CancellationToken ct) =>
        Task.Run(async () => await LoopAsync(ct), ct);

    private async Task LoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && !queue.IsEmpty)
            await TryDequeue(ct);
    }

    private async Task TryDequeue(CancellationToken ct)
    {
        if (queue.TryDequeue(out var input))
        {
            try
            {
                await ProcessInput(input, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Bundling failed for input {Input}.", input);
            }

            Counters.Done.Increment();
        }
        else
        {
            await Task.Delay(10, ct);
        }
    }

    private async Task ProcessInput(string directoryPath, CancellationToken ct)
    {
        if (!EnsureDirectory(directoryPath))
            return;

        await Parallel.ForEachAsync(
            EnumerateDirectories(directoryPath),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = config.Value.MaxDegreeOfParallelism,
                CancellationToken = ct,
            },
            async (dir, ctp) => await ProcessYear(dir, ctp)
        );
    }

    private async ValueTask ProcessYear(DirectoryInfo directory, CancellationToken ct)
    {
        await Parallel.ForEachAsync(
            directory.EnumerateDirectories(),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = config.Value.MaxDegreeOfParallelism,
                CancellationToken = ct,
            },
            async (dir, ctp) => await ProcessMonth(dir, ctp)
        );
    }

    private async ValueTask ProcessMonth(DirectoryInfo directory, CancellationToken ct)
    {
        await Parallel.ForEachAsync(
            directory.EnumerateDirectories(),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = config.Value.MaxDegreeOfParallelism,
                CancellationToken = ct,
            },
            async (dir, ctp) => await ProcessDay(dir, ctp)
        );
    }

    private async ValueTask ProcessDay(DirectoryInfo directory, CancellationToken ct)
    {
        await Parallel.ForEachAsync(
            directory.EnumerateDirectories(),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = config.Value.MaxDegreeOfParallelism,
                CancellationToken = ct,
            },
            async (dir, _) => await ProcessSession(dir)
        );

        var dayFiles = directory.EnumerateFiles();
        var date = DateHelper.GetDateFromExtraction(directory);
        EmitBundle(dayFiles, date, DateRanges.Day, DataKind.Day);
    }

    private ValueTask ProcessSession(DirectoryInfo directory)
    {
        var sessionFiles = directory.EnumerateFiles();
        var date = DateHelper.GetDatetimeFromExtraction(directory);
        EmitBundle(sessionFiles, date, DateRanges.Time, DataKind.Session);
        return ValueTask.CompletedTask;
    }

    private void EmitBundle(
        IEnumerable<FileInfo> files,
        DateTime timestamp,
        DateRanges range,
        DataKind kind
    )
    {
        var filepaths = files.Select(f => f.FullName).ToList();
        if (!filepaths.Any())
            return;

        var bundle = new BundleIndex(Version, timestamp, range, kind, filepaths);
        indexer.Set(bundle);
        Counters.Out.Increment();
        ReportResult?.Invoke(this, new(bundle));
    }

    private IEnumerable<DirectoryInfo> EnumerateDirectories(string directoryPath) =>
        Directory.EnumerateDirectories(directoryPath).Select(path => new DirectoryInfo(path));

    private bool EnsureDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
            return true;

        logger.LogError($"Directory {directoryPath} does not exist");
        Counters.Skip.Increment();
        return false;
    }
}
