using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fip.Strive.Web.Components.Pages;

public partial class JobsPage : IDisposable
{
    /// <summary>
    /// A busy run notifies far more often than a screen can usefully change. The same bound the
    /// import page puts on its upload progress.
    /// </summary>
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(200);

    [Inject]
    private IJobReader Reader { get; set; } = default!;

    [Inject]
    private IJobQueue Queue { get; set; } = default!;

    [Inject]
    private IJobNotifier Notifier { get; set; } = default!;

    [Inject]
    private TimeProvider Clock { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private ILogger<JobsPage> Logger { get; set; } = default!;

    private readonly RefreshWindow _window = new(RefreshInterval);
    private readonly CancellationTokenSource _closing = new();

    private IDisposable? _subscription;
    private IReadOnlyList<JobRow>? _jobs;
    private JobCounts _counts = new(0, 0, 0, 0, 0);

    protected override async Task OnInitializedAsync()
    {
        await RefreshAsync();

        // Subscribed after the first read, so a notification arriving mid-load cannot race it.
        _subscription = Notifier.Subscribe(RequestRefresh);
    }

    private void RequestRefresh()
    {
        if (_window.Request(Clock.GetUtcNow()) is { } delay)
            _ = RefreshAfterAsync(delay);
    }

    /// <summary>
    /// Waits out the coalescing window, then reads. Reading on the way in rather than on the way
    /// out would drop the last notification of a burst — the one that says the job finished.
    /// </summary>
    private async Task RefreshAfterAsync(TimeSpan delay)
    {
        try
        {
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, Clock, _closing.Token);

            await InvokeAsync(RefreshAsync);
        }
        catch (OperationCanceledException)
        {
            // The page went away while we were waiting.
        }
        catch (ObjectDisposedException)
        {
            // Likewise, and the renderer noticed first.
        }
        catch (Exception exception)
        {
            // Nothing awaits this task, so without logging here a failed read is invisible: the
            // page simply stops updating and the fault surfaces, if at all, as an unobserved task
            // exception at some later collection.
            Logger.LogError(exception, "Refreshing the jobs page failed");
        }
        finally
        {
            // A notification that landed after the read above has nothing else to bring it to the
            // screen — the job it announced has finished and will not notify again — so the window
            // hands back a delay and the burst continues until a read goes by unannounced.
            if (_window.Completed(Clock.GetUtcNow()) is { } again)
                _ = RefreshAfterAsync(again);
        }
    }

    private async Task RefreshAsync()
    {
        // Re-read rather than trust the notification's payload: there is none, deliberately.
        _counts = await Reader.GetCountsAsync();
        _jobs = await Reader.GetJobsAsync();

        StateHasChanged();
    }

    private async Task RetryAsync(JobRow job)
    {
        // No payload: the queue keeps the stored one, which for an unpack job is the only record
        // of where its archive is.
        await Queue.EnqueueAsync(job.Kind, job.TargetKey);
        Snackbar.Add($"{job.Kind} job re-queued.", Severity.Info);

        await RefreshAsync();
    }

    private string FormatDuration(JobRow job) =>
        job.DurationAt(Clock.GetUtcNow()) is { } duration ? $"{duration.TotalSeconds:N1}s" : "—";

    private static Color ColorFor(JobState state) =>
        state switch
        {
            JobState.Running => Color.Info,
            JobState.Succeeded => Color.Success,
            JobState.Failed => Color.Error,
            JobState.Stale => Color.Warning,
            _ => Color.Default,
        };

    private static string Shorten(string targetKey) =>
        targetKey.Length <= 16 ? targetKey : targetKey[..16];

    public void Dispose()
    {
        // Unsubscribed first, so nothing schedules a refresh against a cancelled token.
        _subscription?.Dispose();
        _closing.Cancel();
        _closing.Dispose();
    }
}
