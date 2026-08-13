using Fip.Strive.Tracking.Application;
using Fip.Strive.Tracking.Application.Features.Events.Models;
using Fip.Strive.Tracking.Application.Features.Events.Services.Contracts;
using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Fip.Strive.Tracking.Application.Features.Trackers.Services.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fip.Strive.Tracking.Web.Components.Pages;

public partial class TrackerPage
{
    [Parameter]
    public Guid TrackerId { get; set; }

    [Inject]
    private ITrackerReader Trackers { get; set; } = default!;

    [Inject]
    private IEventReader Events { get; set; } = default!;

    [Inject]
    private IEventRecorder Recorder { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private TimeProvider Clock { get; set; } = default!;

    private readonly Dictionary<Guid, decimal?> _numbers = [];
    private readonly Dictionary<Guid, string?> _texts = [];

    private MudTable<EventRow>? _table;
    private TrackerDetail? _tracker;
    private IReadOnlyList<NumberStats>? _stats;
    private DateTime? _date;
    private TimeSpan? _time;
    private string? _note;
    private bool _missing;
    private bool _busy;

    /// <summary>
    /// The tracker's fields, or none while it is still loading — the markup drives three loops off
    /// this and none of them wants to know about the nullable field behind it.
    /// </summary>
    private IReadOnlyList<TrackerFieldRow> Fields => _tracker?.Fields ?? [];

    /// <summary>
    /// Runs on navigation between trackers too, not just on first render — the route parameter
    /// changes while the component instance stays put.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        if (_tracker?.Id == TrackerId)
            return;

        _tracker = await Trackers.GetTrackerAsync(TrackerId);
        _missing = _tracker is null;

        if (_missing)
            return;

        ResetForm();
        await RefreshAsync();
    }

    private decimal? NumberFor(TrackerFieldRow field) =>
        _numbers.TryGetValue(field.Id, out var number) ? number : null;

    private string? TextFor(TrackerFieldRow field) =>
        _texts.TryGetValue(field.Id, out var text) ? text : null;

    private void ResetForm()
    {
        var now = Clock.GetLocalNow();

        _date = now.Date;
        _time = now.TimeOfDay;
        _note = null;

        _numbers.Clear();
        _texts.Clear();
    }

    private async Task RecordAsync()
    {
        _busy = true;

        try
        {
            await Recorder.RecordAsync(TrackerId, new NewEvent(Occurred(), _note, CollectValues()));

            Snackbar.Add("Recorded.", Severity.Success);
            ResetForm();
            await RefreshAsync();
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

    private async Task DeleteAsync(EventRow occurrence)
    {
        _busy = true;

        try
        {
            await Recorder.DeleteAsync(occurrence.Id);
            Snackbar.Add("Event deleted.", Severity.Info);
            await RefreshAsync();
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task<TableData<EventRow>> LoadAsync(
        TableState state,
        CancellationToken cancellationToken
    )
    {
        var page = await Events.GetEventsAsync(
            TrackerId,
            state.Page * state.PageSize,
            state.PageSize,
            cancellationToken
        );

        return new TableData<EventRow> { Items = page.Items, TotalItems = page.TotalCount };
    }

    /// <summary>
    /// The picker hands back a wall-clock date and time with no zone attached. This is a homelab
    /// app with one user, so "the server's zone" is the honest reading of what they typed.
    /// </summary>
    private DateTimeOffset Occurred()
    {
        var wallClock = DateTime.SpecifyKind(
            (_date ?? Clock.GetLocalNow().Date).Date + (_time ?? TimeSpan.Zero),
            DateTimeKind.Unspecified
        );

        return new DateTimeOffset(wallClock, Clock.LocalTimeZone.GetUtcOffset(wallClock));
    }

    private List<NewEventValue> CollectValues() =>
        Fields
            .Select(field =>
                field.Type == TrackerFieldType.Number
                    ? new NewEventValue(field.Id, Number: NumberFor(field))
                    : new NewEventValue(field.Id, Text: TextFor(field))
            )
            .ToList();

    private async Task RefreshAsync()
    {
        await RefreshStatsAsync();

        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task RefreshStatsAsync() => _stats = await Events.GetNumberStatsAsync(TrackerId);
}
