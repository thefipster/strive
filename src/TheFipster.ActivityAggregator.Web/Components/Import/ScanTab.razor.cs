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

public partial class ScanTab : ComponentBase
{
    private HubConnection? _hubConnection;
    private MudTable<FileIndex>? _fileTable;

    private bool _isScanActive;
    private double _progress;
    private string? _progressMessage;

    private string[]? _classifiers;
    private readonly string[] _ranges = Enum.GetNames(typeof(DateRanges));

    private string _selectedClassifiedFilter = "All";
    private string _selectedRangeFilter = "All";
    private string _searchFilter = string.Empty;

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public ScanApi? Scanner { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await ConnectHubs();
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Scanner != null)
            _classifiers = (await Scanner.GetClassifiers())
                .Select(x => x.Key.ToString())
                .OrderBy(x => x)
                .ToArray();

        await base.OnParametersSetAsync();
    }

    private async Task OnScanClicked()
    {
        if (Scanner == null)
            return;

        _isScanActive = true;
        await Scanner.ExecuteFileScan();
    }

    private void OnSearch(string text)
    {
        _searchFilter = text;
        _fileTable?.ReloadServerData();
    }

    private void OnClassificationFilterChanged(string classification)
    {
        _selectedClassifiedFilter = classification;
        _fileTable?.ReloadServerData();
    }

    private void OnRangeFilterChanged(string range)
    {
        _selectedRangeFilter = range;
        _fileTable?.ReloadServerData();
    }

    private async Task<TableData<FileIndex>> LoadServerData(TableState state, CancellationToken ct)
    {
        if (Scanner == null)
            return new TableData<FileIndex> { TotalItems = 0, Items = [] };

        var page = await Scanner.GetFilesPageAsync(
            new PagedRequest(state.Page, state.PageSize),
            _selectedRangeFilter,
            _selectedClassifiedFilter,
            _searchFilter
        );

        return new TableData<FileIndex> { TotalItems = page.Total, Items = page.Items };
    }

    private async Task ConnectHubs()
    {
        if (Navigation == null)
            return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7098" + Defaults.Hubs.Importer.Url)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string, bool>(
            Defaults.Hubs.Importer.ReportAction,
            (_, update) =>
            {
                if (update)
                {
                    _progress = 0;
                    _isScanActive = false;
                    _progressMessage = null;

                    _fileTable?.ReloadServerData();
                    InvokeAsync(StateHasChanged);
                }
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

        await _hubConnection.InvokeAsync("JoinGroup", Defaults.Hubs.Importer.Actions.Scan);
    }
}
