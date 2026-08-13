using Fip.Strive.Tracking.Application;
using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Fip.Strive.Tracking.Application.Features.Trackers.Services.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fip.Strive.Tracking.Web.Components.Pages;

public partial class TrackerFieldsPage
{
    [Parameter]
    public Guid TrackerId { get; set; }

    [Inject]
    private ITrackerReader Trackers { get; set; } = default!;

    [Inject]
    private ITrackerWriter Writer { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private TrackerDetail? _tracker;
    private string? _name;
    private string? _description;
    private string? _fieldName;
    private string? _fieldUnit;
    private TrackerFieldType _fieldType = TrackerFieldType.Number;
    private bool _fieldRequired;
    private bool _confirmingDelete;
    private bool _missing;
    private bool _busy;

    protected override async Task OnParametersSetAsync()
    {
        if (_tracker?.Id == TrackerId)
            return;

        await ReloadAsync();
    }

    private async Task AddFieldAsync()
    {
        _busy = true;

        try
        {
            await Writer.AddFieldAsync(
                TrackerId,
                new NewField(
                    _fieldName ?? string.Empty,
                    _fieldType,
                    // A unit only ever renders next to a number, so carrying one on a text field
                    // would just be a setting that does nothing.
                    _fieldType == TrackerFieldType.Number ? _fieldUnit : null,
                    _fieldRequired
                )
            );

            _fieldName = null;
            _fieldUnit = null;
            _fieldRequired = false;

            await ReloadAsync();
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

    private async Task ToggleRequiredAsync(TrackerFieldRow field, bool required)
    {
        _busy = true;

        try
        {
            await Writer.UpdateFieldAsync(
                field.Id,
                new FieldEdit(field.Name, field.Unit, required)
            );

            await ReloadAsync();
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

    private async Task DeleteFieldAsync(TrackerFieldRow field)
    {
        _busy = true;

        try
        {
            await Writer.DeleteFieldAsync(field.Id);
            Snackbar.Add($"'{field.Name}' and everything recorded under it is gone.", Severity.Info);
            await ReloadAsync();
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

    private async Task SaveTrackerAsync()
    {
        _busy = true;

        try
        {
            await Writer.UpdateTrackerAsync(
                TrackerId,
                new TrackerEdit(_name ?? string.Empty, _description)
            );

            Snackbar.Add("Saved.", Severity.Success);
            await ReloadAsync();
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

    private async Task DeleteTrackerAsync()
    {
        _busy = true;

        try
        {
            await Writer.DeleteTrackerAsync(TrackerId);
            Snackbar.Add($"'{_tracker?.Name}' deleted.", Severity.Info);
            Navigation.NavigateTo("/");
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

    private async Task ReloadAsync()
    {
        _tracker = await Trackers.GetTrackerAsync(TrackerId);
        _missing = _tracker is null;
        _confirmingDelete = false;

        // The tracker form edits what is stored, so it is refilled from storage rather than left
        // holding whatever the last failed attempt typed.
        _name = _tracker?.Name;
        _description = _tracker?.Description;
    }
}
