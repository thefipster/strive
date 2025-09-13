using System.Collections.Concurrent;
using Fipster.TrackMe.Domain.Exceptions;
using Fipster.TrackMe.Domain.Extensions;
using Fipster.TrackMe.Domain.Models.Indexes;
using Fipster.TrackMe.Importer.Modules.Abstractions;
using Fipster.TrackMe.Pipeline.Abstractions;
using Fipster.TrackMe.Pipeline.Config;
using Fipster.TrackMe.Pipeline.Models;
using Microsoft.Extensions.Options;

namespace Fipster.TrackMe.Pipeline.Services;

public class ScannerService(IOptions<ScannerConfig> scannerConfig, IImporterRegistry registry)
    : IScannerService
{
    private readonly ScannerConfig scannerConfig = scannerConfig.Value;

    private readonly IEnumerable<IFileClassifier> classifiers = registry.LoadClassifiers();

    private readonly ConcurrentQueue<ImportFile> queue = new();
    private readonly ConcurrentBag<int> total = new();
    private readonly ConcurrentBag<int> errors = new();
    private readonly ConcurrentBag<int> processed = new();

    private volatile bool scannerFinished;
    private volatile bool classifierFinished;

    public event EventHandler<ProgressReportEventArgs>? ReportProgress;
    public event EventHandler<ErrorReportEventArgs>? ReportError;
    public event EventHandler<ImportReadyEventArgs>? ReportImport;

    public async Task ExecuteAsync(CancellationToken token = default)
    {
        var tasks = new List<Task>();

        tasks.Add(CreateScannerTask(token));
        tasks.Add(CreateReportTask(token));
        tasks.Add(CreateClassificationTask(token));

        await Task.WhenAll(tasks);
    }

    private Task CreateClassificationTask(CancellationToken token = default)
    {
        var classifyTask = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested && (!scannerFinished || !queue.IsEmpty))
            {
                if (!queue.TryDequeue(out var import))
                {
                    await Task.Delay(10, token);
                    continue;
                }

                var hash = import.File.GetMd5Hash();
                var results = new ConcurrentBag<ImportClassification>();

                Parallel.ForEach(
                    classifiers,
                    classifier =>
                    {
                        var filepath = import.File.FullName;
                        try
                        {
                            var result = classifier.Classify(filepath);
                            if (result != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            ReportError?.Invoke(this, new ErrorReportEventArgs("scanner", e));
                        }
                    }
                );

                var index = new ImportIndex(
                    import.File.FullName,
                    hash,
                    results.Select(x => x.ToClassification())
                );

                ReportClassificationResult(index);
            }

            classifierFinished = true;
        });

        return classifyTask;
    }

    private Task CreateScannerTask(CancellationToken token = default)
    {
        var scanTask = Task.Run(
            () =>
            {
                Parallel.ForEach(
                    scannerConfig.Include,
                    new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = token },
                    directoryPath =>
                    {
                        var directory = new DirectoryInfo(directoryPath);
                        var files = directory.EnumerateFiles("*", SearchOption.AllDirectories);
                        Parallel.ForEach(
                            files,
                            file =>
                            {
                                var import = new ImportFile(directory, file);
                                queue.Enqueue(import);
                                total.Add(1);
                            }
                        );
                    }
                );

                scannerFinished = true;
            },
            token
        );

        return scanTask;
    }

    private Task CreateReportTask(CancellationToken token = default)
    {
        var reportTask = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested && (!scannerFinished || !classifierFinished))
            {
                await Task.Delay(TimeSpan.FromSeconds(1), token);
                ReportProgress?.Invoke(
                    this,
                    new ProgressReportEventArgs(
                        "scanner",
                        total.Count,
                        processed.Count,
                        errors.Count
                    )
                );
            }
        });

        return reportTask;
    }

    private void ReportClassificationResult(ImportIndex index)
    {
        if (!index.Classifications.Any())
        {
            errors.Add(1);
            ReportError?.Invoke(
                this,
                new ErrorReportEventArgs(
                    "scanner",
                    new ClassificationException(index.Filepath, "No classification.")
                )
            );
        }

        if (index.Classifications.Count() > 1)
        {
            errors.Add(1);
            ReportError?.Invoke(
                this,
                new ErrorReportEventArgs(
                    "scanner",
                    new ClassificationException(index.Filepath, "Multiple classifications.")
                )
            );
        }

        if (index.Classifications.Count() == 1)
        {
            processed.Add(1);
            ReportImport?.Invoke(this, new ImportReadyEventArgs(index));
        }
    }
}
