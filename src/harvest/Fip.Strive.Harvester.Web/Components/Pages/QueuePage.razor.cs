using Fip.Strive.Queue.Domain.Enums;
using Fip.Strive.Queue.Domain.Models;
using Fip.Strive.Queue.Storage.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fip.Strive.Harvester.Web.Components.Pages;

[Route("/queue")]
public partial class QueuePage(IJobReader jobReader) : ComponentBase
{
    private bool _dialogVisible;
    private string _selectedError = string.Empty;
    private string _selectedPayload = string.Empty;

    private readonly DialogOptions _dialogOptions = new()
    {
        FullWidth = true,
        MaxWidth = MaxWidth.Large,
        CloseButton = true,
        CloseOnEscapeKey = true,
    };

    private MudTable<JobDetails>? _activeTable;
    private MudTable<JobDetails>? _doneTable;

    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;

    protected override void OnInitialized()
    {
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));
        _ = RunUpdateLookAsync(_cts.Token);
    }

    private async Task RunUpdateLookAsync(CancellationToken ct)
    {
        try
        {
            while (
                !ct.IsCancellationRequested
                && _timer is not null
                && await _timer.WaitForNextTickAsync(ct)
            )
            {
                _activeTable?.ReloadServerData();
                _doneTable?.ReloadServerData();

                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
    }

    private Task<TableData<JobDetails>> OnActiveRequested(TableState state, CancellationToken ct)
    {
        var result = jobReader.GetJobs(
            state.Page,
            state.PageSize,
            JobStatus.Stored,
            JobStatus.Pending,
            JobStatus.Running
        );
        return Task.FromResult(
            new TableData<JobDetails> { Items = result.Items, TotalItems = result.Total }
        );
    }

    private Task<TableData<JobDetails>> OnDoneRequested(TableState state, CancellationToken ct)
    {
        var result = jobReader.GetJobs(
            state.Page,
            state.PageSize,
            JobStatus.Succeeded,
            JobStatus.Failed
        );
        return Task.FromResult(
            new TableData<JobDetails> { Items = result.Items, TotalItems = result.Total }
        );
    }

    private void OnRowClicked(TableRowClickEventArgs<JobDetails> obj)
    {
        var job = obj.Item;
        if (job == null)
            return;

        _selectedError = job.Error ?? string.Empty;
        _selectedPayload = job.Payload ?? string.Empty;
        _dialogVisible = true;
        StateHasChanged();
    }
}
