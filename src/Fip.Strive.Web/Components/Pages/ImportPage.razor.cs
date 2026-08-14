using Fip.Strive.Application.Features.Catalog.Models;
using Fip.Strive.Application.Features.Catalog.Services.Contracts;
using Fip.Strive.Application.Features.Import.Services;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
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
    /// Progress arrives per byte; re-rendering that often would swamp the circuit.
    /// </summary>
    private static readonly TimeSpan RenderInterval = TimeSpan.FromMilliseconds(200);

    [Inject]
    private IStagingArea Staging { get; set; } = default!;

    [Inject]
    private ICatalogReader Catalog { get; set; } = default!;

    [Inject]
    private IJobQueue Jobs { get; set; } = default!;

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

    /// <summary>
    /// Uploads and queues, and that is all. Unpacking belongs to the job engine now, so nothing
    /// here waits on it and closing the tab cannot abandon it.
    /// </summary>
    private async Task ImportAsync(IBrowserFile file, CancellationToken cancellationToken)
    {
        var job = new UploadJob(file.Name, file.Size);
        _jobs.Insert(0, job);
        await InvokeAsync(StateHasChanged);

        try
        {
            var staged = await StageAsync(file, job, cancellationToken);
            await QueueAsync(staged, job);

            Snackbar.Add($"{file.Name} queued for unpacking.", Severity.Success);
        }
        catch (OperationCanceledException)
        {
            job.Phase = UploadPhase.Cancelled;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Upload of {FileName} failed", file.Name);
            job.Phase = UploadPhase.Failed;
            job.Error = exception.Message;
            Snackbar.Add($"{file.Name} failed: {exception.Message}", Severity.Error);
        }
        finally
        {
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

    /// <summary>
    /// Keyed by the archive hash, so re-uploading the same bytes reuses the work unit rather than
    /// queueing a second one. The staged file is the handler's to delete from here on.
    /// </summary>
    private async Task QueueAsync(StagedArchive staged, UploadJob job)
    {
        await Jobs.EnqueueAsync(UnpackJobHandler.JobKind, staged.Hash, staged);

        job.Phase = UploadPhase.Queued;
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
            UploadPhase.Queued => Icons.Material.Outlined.Schedule,
            UploadPhase.Failed => Icons.Material.Outlined.ErrorOutline,
            UploadPhase.Cancelled => Icons.Material.Outlined.Cancel,
            _ => Icons.Material.Outlined.HourglassTop,
        };

    private static Color ColorFor(UploadJob job) =>
        job.Phase switch
        {
            UploadPhase.Queued => Color.Info,
            UploadPhase.Failed => Color.Error,
            UploadPhase.Cancelled => Color.Warning,
            _ => Color.Default,
        };

    public void Dispose() => _cancellation?.Dispose();

    private enum UploadPhase
    {
        Uploading,
        Queued,
        Cancelled,
        Failed,
    }

    private sealed class UploadJob(string fileName, long sizeBytes)
    {
        public string FileName { get; } = fileName;

        public long SizeBytes { get; } = sizeBytes;

        public UploadPhase Phase { get; set; } = UploadPhase.Uploading;

        public long BytesUploaded { get; set; }

        public string? Error { get; set; }

        public bool ShowsProgress => Phase is UploadPhase.Uploading;

        public double Percent =>
            Phase switch
            {
                UploadPhase.Uploading => SizeBytes == 0 ? 0 : 100d * BytesUploaded / SizeBytes,
                _ => 100,
            };

        public string Detail =>
            Phase switch
            {
                UploadPhase.Uploading => $"Uploading — {ByteSize.Format(BytesUploaded)}",
                UploadPhase.Queued => "Queued for unpacking",
                UploadPhase.Cancelled => "Cancelled",
                UploadPhase.Failed => Error ?? "Failed",
                _ => string.Empty,
            };
    }
}
