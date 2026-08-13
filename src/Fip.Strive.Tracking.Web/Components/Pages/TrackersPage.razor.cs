using Fip.Strive.Tracking.Application;
using Fip.Strive.Tracking.Application.Features.Events.Models;
using Fip.Strive.Tracking.Application.Features.Events.Services.Contracts;
using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Fip.Strive.Tracking.Application.Features.Trackers.Services.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fip.Strive.Tracking.Web.Components.Pages;

public partial class TrackersPage
{
    [Inject]
    private ITrackerReader Trackers { get; set; } = default!;

    [Inject]
    private ITrackerWriter Writer { get; set; } = default!;

    [Inject]
    private IEventRecorder Recorder { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private TimeProvider Clock { get; set; } = default!;

    private IReadOnlyList<TrackerRow>? _trackers;
    private string? _name;
    private string? _description;
    private bool _creating;
    private bool _busy;

    protected override async Task OnInitializedAsync() => await ReloadAsync();

    private static string Detail(TrackerRow tracker) => $"/trackers/{tracker.Id}";

    private void ToggleForm()
    {
        _creating = !_creating;
        _name = null;
        _description = null;
    }

    private async Task CreateAsync()
    {
        _busy = true;

        try
        {
            var id = await Writer.CreateTrackerAsync(
                new NewTracker(_name ?? string.Empty, _description)
            );

            // Straight on to the field editor: a tracker without its custom fields is half set up,
            // and adding them after the first events means those events have gaps.
            Navigation.NavigateTo($"/trackers/{id}/fields");
        }
        catch (TrackingException exception)
        {
            Snackbar.Add(exception.Message, Severity.Warning);
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task LogNowAsync(TrackerRow tracker)
    {
        _busy = true;

        try
        {
            await Recorder.RecordAsync(tracker.Id, new NewEvent(Clock.GetUtcNow()));
            Snackbar.Add($"Logged {tracker.Name}.", Severity.Success);
            await ReloadAsync();
        }
        catch (TrackingException exception)
        {
            // A field turned required in another tab, or the tracker is gone. Either way the
            // detail page is where the user can see what happened.
            Snackbar.Add(exception.Message, Severity.Warning);
            Navigation.NavigateTo(Detail(tracker));
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task ReloadAsync() => _trackers = await Trackers.GetTrackersAsync();
}
