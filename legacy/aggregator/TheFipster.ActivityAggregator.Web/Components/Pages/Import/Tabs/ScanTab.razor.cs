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

public partial class ScanTab : ComponentBase
{
    private MudTable<FileIndex>? _fileTable;

    private HubConnection? _hubConnection;
    private readonly Subject<(int, int, int, double)> _queueEvents = new();

    private string[]? _classifiers;
    private readonly string[] _ranges = Enum.GetNames(typeof(DateRanges));

    private string _selectedClassifiedFilter = "All";
    private string _selectedRangeFilter = "All";
    private string _searchFilter = string.Empty;

    [Inject]
    public required ScanApi Scanner { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await ConnectHubs();
    }

    protected override async Task OnParametersSetAsync()
    {
        _classifiers = (await Scanner.GetClassifiers())
            .Select(x => x.Key.ToString())
            .OrderBy(x => x)
            .ToArray();
    }

    private async Task OnScanClicked()
    {
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
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7098" + Const.Hubs.Importer.Url)
            .WithAutomaticReconnect()
            .Build();

        _queueEvents
            .Buffer(count: 10)
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
