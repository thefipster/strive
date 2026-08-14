using Fip.Strive.Application.Features.Jobs.Models;
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

    private IDisposable? _subscription;
    private IReadOnlyList<JobRow>? _jobs;
    private JobCounts _counts = new(0, 0, 0, 0, 0);
    private DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;

    protected override async Task OnInitializedAsync()
    {
        await RefreshAsync();

        // Subscribed after the first read, so a notification arriving mid-load cannot race it.
        _subscription = Notifier.Subscribe(RequestRefresh);
    }

    private void RequestRefresh()
    {
        var now = Clock.GetUtcNow();

        if (now - _lastRefresh < RefreshInterval)
            return;

        _lastRefresh = now;
        _ = InvokeAsync(RefreshAsync);
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

    public void Dispose() => _subscription?.Dispose();
}
