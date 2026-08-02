using Fip.Strive.Application.Features.Catalog.Models;
using Fip.Strive.Application.Features.Catalog.Services.Contracts;
using Fip.Strive.Application.Features.Import.Models;
using Fip.Strive.Application.Features.Import.Services.Contracts;
using Fip.Strive.Application.Features.Storage;
using Fip.Strive.Application.Features.Storage.Models;
using Fip.Strive.Application.Features.Storage.Services.Contracts;
using Fip.Strive.Web.Components.Parts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using MudBlazor;

namespace Fip.Strive.Web.Components.Pages;

public partial class ImportPage : IDisposable
{
    /// <summary>
    /// Progress arrives per file and per byte; re-rendering that often would swamp the circuit.
    /// </summary>
    private static readonly TimeSpan RenderInterval = TimeSpan.FromMilliseconds(200);

    [Inject]
    private IStagingArea Staging { get; set; } = default!;

    [Inject]
    private ICatalogReader Catalog { get; set; } = default!;

    [Inject]
    private IServiceScopeFactory ScopeFactory { get; set; } = default!;

    [Inject]
    private IOptions<StorageOptions> StorageOptions { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private ILogger<ImportPage> Logger { get; set; } = default!;

    private readonly List<UploadJob> _jobs = [];

    private CancellationTokenSource? _cancellation;
    private CatalogSummary _summary = new(0, 0, 0);
    private DateTimeOffset _lastRender = DateTimeOffset.MinValue;
    private bool _busy;

    private long MaxUploadBytes => StorageOptions.Value.MaxUploadBytes;

    protected override async Task OnInitializedAsync() => await RefreshSummaryAsync();

    private async Task OnFilesChangedAsync(IReadOnlyList<IBrowserFile> files)
    {
        if (_busy || files.Count == 0)
            return;

        _busy = true;
        _cancellation = new CancellationTokenSource();

        try
        {
            foreach (var file in files)
                await ImportAsync(file, _cancellation.Token);
        }
        finally
        {
            _cancellation.Dispose();
            _cancellation = null;
            _busy = false;
            await RefreshSummaryAsync();
        }
    }

    private async Task ImportAsync(IBrowserFile file, CancellationToken cancellationToken)
    {
        var job = new UploadJob(file.Name, file.Size);
        _jobs.Insert(0, job);
        await InvokeAsync(StateHasChanged);

        StagedArchive? staged = null;

        try
        {
            staged = await StageAsync(file, job, cancellationToken);
            var result = await UnpackAsync(staged, job, cancellationToken);
            Announce(job, result);
        }
        catch (OperationCanceledException)
        {
            job.Phase = UploadPhase.Cancelled;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Import of {FileName} failed", file.Name);
            job.Phase = UploadPhase.Failed;
            job.Error = exception.Message;
            Snackbar.Add($"{file.Name} failed: {exception.Message}", Severity.Error);
        }
        finally
        {
            // The archive's contents are in the blob store now; keeping the ZIP too would just
            // double the disk bill.
            if (staged is not null)
                Staging.Discard(staged);

            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task<StagedArchive> StageAsync(
        IBrowserFile file,
        UploadJob job,
        CancellationToken cancellationToken
    )
    {
        var progress = new Progress<long>(bytes =>
        {
            job.BytesUploaded = bytes;
            RequestRender();
        });

        await using var stream = file.OpenReadStream(MaxUploadBytes, cancellationToken);
        return await Staging.StageAsync(file.Name, stream, progress, cancellationToken);
    }

    private async Task<ImportResult> UnpackAsync(
        StagedArchive staged,
        UploadJob job,
        CancellationToken cancellationToken
    )
    {
        job.Phase = UploadPhase.Unpacking;
        await InvokeAsync(StateHasChanged);

        var progress = new Progress<ImportProgress>(update =>
        {
            job.FilesProcessed = update.FilesProcessed;
            job.TotalFiles = update.TotalFiles;
            RequestRender();
        });

        // A scope per import keeps the DbContext short-lived instead of living as long as the
        // circuit. Step 2 moves this onto the job engine entirely.
        await using var scope = ScopeFactory.CreateAsyncScope();
        var importer = scope.ServiceProvider.GetRequiredService<IPackageImporter>();

        return await importer.ImportAsync(staged, progress, cancellationToken);
    }

    private void Announce(UploadJob job, ImportResult result)
    {
        job.Result = result;
        job.Phase =
            result.Outcome == ImportOutcome.DuplicateArchive
                ? UploadPhase.Duplicate
                : UploadPhase.Imported;

        if (result.Outcome == ImportOutcome.DuplicateArchive)
        {
            Snackbar.Add($"{job.FileName} was already imported — nothing to do.", Severity.Info);
            return;
        }

        Snackbar.Add(
            $"{job.FileName}: {result.FileCount} files, {result.NewEntryCount} new.",
            Severity.Success
        );
    }

    private void Cancel() => _cancellation?.Cancel();

    private async Task RefreshSummaryAsync()
    {
        _summary = await Catalog.GetSummaryAsync();
        await InvokeAsync(StateHasChanged);
    }

    private void RequestRender()
    {
        var now = DateTimeOffset.UtcNow;

        if (now - _lastRender < RenderInterval)
            return;

        _lastRender = now;
        _ = InvokeAsync(StateHasChanged);
    }

    private static string IconFor(UploadJob job) =>
        job.Phase switch
        {
            UploadPhase.Imported => Icons.Material.Outlined.CheckCircle,
            UploadPhase.Duplicate => Icons.Material.Outlined.ContentCopy,
            UploadPhase.Failed => Icons.Material.Outlined.ErrorOutline,
            UploadPhase.Cancelled => Icons.Material.Outlined.Cancel,
            _ => Icons.Material.Outlined.HourglassTop,
        };

    private static Color ColorFor(UploadJob job) =>
        job.Phase switch
        {
            UploadPhase.Imported => Color.Success,
            UploadPhase.Duplicate => Color.Info,
            UploadPhase.Failed => Color.Error,
            UploadPhase.Cancelled => Color.Warning,
            _ => Color.Default,
        };

    public void Dispose() => _cancellation?.Dispose();

    private enum UploadPhase
    {
        Uploading,
        Unpacking,
        Imported,
        Duplicate,
        Cancelled,
        Failed,
    }

    private sealed class UploadJob(string fileName, long sizeBytes)
    {
        public string FileName { get; } = fileName;

        public long SizeBytes { get; } = sizeBytes;

        public UploadPhase Phase { get; set; } = UploadPhase.Uploading;

        public long BytesUploaded { get; set; }

        public int FilesProcessed { get; set; }

        public int TotalFiles { get; set; }

        public ImportResult? Result { get; set; }

        public string? Error { get; set; }

        public bool ShowsProgress => Phase is UploadPhase.Uploading or UploadPhase.Unpacking;

        public double Percent =>
            Phase switch
            {
                UploadPhase.Uploading => SizeBytes == 0 ? 0 : 100d * BytesUploaded / SizeBytes,
                UploadPhase.Unpacking => TotalFiles == 0 ? 0 : 100d * FilesProcessed / TotalFiles,
                _ => 100,
            };

        public string Detail =>
            Phase switch
            {
                UploadPhase.Uploading => $"Uploading — {ByteSize.Format(BytesUploaded)}",
                UploadPhase.Unpacking => $"Unpacking — {FilesProcessed} of {TotalFiles} files",
                UploadPhase.Imported => Result is null
                    ? "Imported"
                    : $"{Result.FileCount} files, {Result.NewEntryCount} new, "
                        + $"{Result.FileCount - Result.NewEntryCount} already known",
                UploadPhase.Duplicate => "Already imported — no work done",
                UploadPhase.Cancelled => "Cancelled",
                UploadPhase.Failed => Error ?? "Failed",
                _ => string.Empty,
            };
    }
}
