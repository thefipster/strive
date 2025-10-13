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
    private HubConnection? _hubConnection;
    private MudTable<ExtractorIndex>? _fileTable;

    private bool _isAssimilationActive;
    private double _progress;
    private string? _progressMessage;

    private string[]? _extractors;
    private readonly string[] _parameters = Enum.GetNames(typeof(Parameters));

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

        _isAssimilationActive = true;
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

        _hubConnection.On<string, bool>(
            Defaults.Hubs.Importer.ReportAction,
            (_, updated) =>
            {
                if (!updated)
                    return;

                _progress = 0;
                _progressMessage = null;
                _isAssimilationActive = false;

                _fileTable?.ReloadServerData();
                InvokeAsync(StateHasChanged);
            }
        );

        _hubConnection.On<string, double>(
            Defaults.Hubs.Importer.ReportProgress,
            (message, progress) =>
            {
                _progress = progress;
                _progressMessage = message;

                _fileTable?.ReloadServerData();
                InvokeAsync(StateHasChanged);
            }
        );

        await _hubConnection.StartAsync();
        await JoinGroups();
        _hubConnection.Reconnected += async _ => await JoinGroups();
    }

    private async Task JoinGroups()
    {
        if (_hubConnection == null)
            return;

        await _hubConnection.InvokeAsync("JoinGroup", Defaults.Hubs.Importer.Actions.Assimilate);
    }
}
