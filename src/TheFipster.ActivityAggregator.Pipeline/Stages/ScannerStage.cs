using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;
using TheFipster.ActivityAggregator.Storage.Abstractions;

namespace TheFipster.ActivityAggregator.Pipeline.Stages;

public class ScannerStage(
    PipelineState state,
    IOptions<ScannerConfig> config,
    IScanIndexer indexer,
    ILogger<ScannerStage> logger
) : IScannerStage
{
    public const string Id = "scanner";
    public int Version => 1;
    public string Name => Id;
    public int Order => 10;

    public ProgressCounters Counters { get; } = new();

    private readonly ConcurrentQueue<string> queue = new();

    private HashSet<string> excludedFileExtensions =
        config.Value.ExcludedFileExtensions.ToHashSet();

    public event EventHandler<ResultReportEventArgs<ScanIndex>>? ReportResult;

    public void Enqueue(string import)
    {
        queue.Enqueue(import);
        Counters.In.Increment();
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        await RunStageAsync(ct);
        state.FinishedStages.Add(Name);
    }

    private async Task RunStageAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && !queue.IsEmpty)
        {
            if (queue.TryDequeue(out var input))
            {
                await ProcessDirectoryAsync(input, ct);
            }
            else
            {
                await Task.Delay(10, ct);
            }
        }
    }

    private async Task ProcessDirectoryAsync(string directoryPath, CancellationToken ct)
    {
        if (!EnsureDirectory(directoryPath))
            return;

        await Parallel.ForEachAsync(
            EnumerateFiles(directoryPath),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = config.Value.MaxDegreeOfParallelism,
                CancellationToken = ct,
            },
            (file, ctp) => ProcessFile(file, directoryPath, ctp)
        );

        Counters.Done.Increment();
    }

    private async ValueTask ProcessFile(FileInfo file, string inputDir, CancellationToken ct)
    {
        if (excludedFileExtensions.Contains(file.Extension))
        {
            Counters.Skip.Increment();
            return;
        }

        var hash = await file.HashXx3Async(ct);
        var index = indexer.GetById(file.FullName);

        if (index == null || index.IndexedAt < file.LastWriteTimeUtc || index.Version < Version)
        {
            index = new ScanIndex(Version, inputDir, file.FullName, hash);
            indexer.Set(index);
            ReportResult?.Invoke(this, new(index));
            Counters.Out.Increment();
        }
        else
        {
            Counters.Skip.Increment();
        }
    }

    private IEnumerable<FileInfo> EnumerateFiles(string directoryPath)
    {
        return Directory
            .EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories)
            .Select(filepath => new FileInfo(filepath));
    }

    private bool EnsureDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
            return true;

        logger.LogError($"Directory {directoryPath} does not exist");
        Counters.Skip.Increment();
        return false;
    }
}
