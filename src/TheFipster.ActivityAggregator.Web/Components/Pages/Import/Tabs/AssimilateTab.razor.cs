using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Pages.Import.Tabs;

public partial class AssimilateTab : ComponentBase
{
    private MudTable<ExtractorIndex>? _fileTable;

    private HubConnection? _hubConnection;
    private readonly Subject<(int, int, int, double)> _queueEvents = new();

    private string[]? _extractors;
    private readonly string[] _parameters = Enum.GetNames(typeof(Parameters))
        .OrderBy(x => x)
        .ToArray();

    private string? _selectedClassifiedFilter;
    private string? _selectedParameterFilter;

    [Inject]
    public required AssimilateApi Api { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await ConnectHubs();
    }

    protected override async Task OnParametersSetAsync()
    {
        _extractors = (await Api.GetExtractors())
            .Select(x => x.Key.ToString())
            .OrderBy(x => x)
            .ToArray();
    }

    private async Task OnAssimilateClicked()
    {
        await Api.ExecuteAssimilation();
    }

    private void OnClassificationFilterChanged(string classification)
    {
        _selectedClassifiedFilter = classification;
        _fileTable?.ReloadServerData();
    }

    private void OnParameterFilterChanged(string parameter)
    {
        _selectedParameterFilter = parameter;
        _fileTable?.ReloadServerData();
    }

    private async Task<TableData<ExtractorIndex>> LoadServerData(
        TableState state,
        CancellationToken ct
    )
    {
        var paged = new PagedRequest(state.Page, state.PageSize);
        var result = await Api.GetExtractsPageAsync(
            paged,
            _selectedClassifiedFilter,
            _selectedParameterFilter
        );

        return new TableData<ExtractorIndex> { TotalItems = result.Total, Items = result.Items };
    }

    private async Task ConnectHubs()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7098" + Const.Hubs.Importer.Url)
            .WithAutomaticReconnect()
            .Build();

        _queueEvents
            .Buffer(count: 9)
            .Select(events => events.Last())
            .Subscribe(_ =>
            {
                _fileTable?.ReloadServerData();
                InvokeAsync(StateHasChanged);
            });

        _hubConnection.On<int, int, int, double>(
            Const.Hubs.Importer.ReportQueue,
            (a, b, c, d) => _queueEvents.OnNext((a, b, c, d))
        );

        await _hubConnection.StartAsync();
        await JoinGroups();
        _hubConnection.Reconnected += async _ => await JoinGroups();
    }

    private async Task JoinGroups()
    {
        if (_hubConnection == null)
            return;

        await _hubConnection.InvokeAsync("JoinGroup", Const.Hubs.Importer.Actions.Queue);
    }
}
