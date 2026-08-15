using System.Text.Json;
using Fip.Strive.Application.Features.Import.Models;
using Fip.Strive.Application.Features.Import.Services.Contracts;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.Application.Features.Storage.Models;
using Fip.Strive.Application.Features.Storage.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Application.Features.Import.Services;

/// <summary>
/// Unpacking, as a job. Idempotent by way of the importer's duplicate-archive check: a crash
/// between the import's commit and the job's terminal write means the retry finds the package
/// already there and does nothing, which is what keeps a kill mid-run from duplicating work.
/// </summary>
public sealed class UnpackJobHandler(
    IPackageImporter importer,
    IStagingArea staging,
    ILogger<UnpackJobHandler> logger
) : IJobHandler
{
    public const string JobKind = "unpack";

    private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

    public string Kind => JobKind;

    public string ComponentId => JobKind;

    /// <summary>
    /// Bumping this would re-unpack every archive, producing byte-identical blobs. It exists for
    /// the shape the later steps need, not because unpacking is expected to change.
    /// </summary>
    public int Version => 1;

    public async Task ExecuteAsync(JobContext context, CancellationToken cancellationToken)
    {
        // The payload is a StagedArchive verbatim — it already carries every field needed to find
        // the archive again after a restart, so a parallel record would only be duplication.
        var archive =
            JsonSerializer.Deserialize<StagedArchive>(
                context.Job.Payload ?? string.Empty,
                PayloadOptions
            )
            ?? throw new InvalidOperationException(
                $"Unpack job {context.Job.Id} carries no usable payload."
            );

        var progress = new Relay(context.Progress);

        var result = await importer.ImportAsync(archive, progress, cancellationToken);

        // Only once the import has committed. A failed job keeps its archive so a manual retry has
        // something to read — leftover bytes are a cheaper problem than an unretryable job.
        staging.Discard(archive);

        logger.LogInformation(
            "Unpacked {FileName}: {Outcome}, {FileCount} files",
            archive.FileName,
            result.Outcome,
            result.FileCount
        );
    }

    /// <summary>
    /// Hands each import report straight to the job's progress on the thread that reported it.
    /// Deliberately not <see cref="Progress{T}"/>: with no synchronisation context to capture — and
    /// a job handler never has one — it posts every callback to the thread pool separately, so the
    /// reports arrive in whatever order the pool runs them and a job can finish displaying a
    /// position it passed several files ago.
    /// </summary>
    private sealed class Relay(IProgress<JobProgress> target) : IProgress<ImportProgress>
    {
        public void Report(ImportProgress value) =>
            target.Report(
                new JobProgress(value.FilesProcessed, value.TotalFiles, value.CurrentPath)
            );
    }
}
