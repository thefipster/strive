using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fip.Strive.Harvester.Application.Core.Hubs;
using Fip.Strive.Queue.Domain.Enums;
using Fip.Strive.Queue.Domain.Models;
using Fip.Strive.Queue.Storage.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace Fip.Strive.Harvester.Web.Components.Pages;

[Route("/queue")]
public partial class QueuePage(IJobReader jobReader, NavigationManager navigation) : ComponentBase
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

    private HubConnection? _hubConnection;
    private readonly Subject<int> _queueEvents = new();
    private MudTable<JobDetails>? _activeTable;
    private MudTable<JobDetails>? _doneTable;

    protected override async Task OnInitializedAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigation.ToAbsoluteUri($"/hubs/{QueueHub.HubName}"))
            .WithAutomaticReconnect()
            .Build();

        _queueEvents
            .Buffer(count: 20)
            .Select(events => events.Last())
            .Subscribe(_ =>
            {
                _activeTable?.ReloadServerData();
                _doneTable?.ReloadServerData();
                InvokeAsync(StateHasChanged);
            });

        _hubConnection.On<int, int, double>(
            QueueHub.QueueReportMethodName,
            (count, _, _) => _queueEvents.OnNext(count)
        );

        await _hubConnection.StartAsync();
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
