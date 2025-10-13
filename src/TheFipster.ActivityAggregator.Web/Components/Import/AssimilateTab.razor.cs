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
using Defaults = TheFipster.ActivityAggregator.Domain.Defaults;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

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
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public AssimilateApi? Api { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await ConnectHubs();
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Api != null)
            _extractors = (await Api.GetExtractors())
                .Select(x => x.Key.ToString())
                .OrderBy(x => x)
                .ToArray();

        await base.OnParametersSetAsync();
    }

    private async Task OnAssimilateClicked()
    {
        if (Api == null)
            return;

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
        if (Api == null)
            return new TableData<ExtractorIndex> { TotalItems = 0, Items = [] };

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
        if (Navigation is null)
            return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7098" + Defaults.Hubs.Importer.Url)
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
