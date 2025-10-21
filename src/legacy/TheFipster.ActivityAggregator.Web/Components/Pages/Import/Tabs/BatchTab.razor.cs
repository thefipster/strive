using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Pages.Import.Tabs;

public partial class BatchTab : ComponentBase
{
    private HubConnection? _hubConnection;
    private readonly Subject<(int, int, int, double)> _queueEvents = new();

    private bool _isRendered;

    private int _selectedYear = DateTime.Now.Year;

    private Dictionary<int, int[]> _index = new();
    private IEnumerable<InventoryIndex> _inventory = [];
    private IEnumerable<DateTime>? _batchDates;

    [Inject]
    public required InventoryApi Inventory { get; set; }

    [Inject]
    public required BatchApi Batch { get; set; }

    [Inject]
    public required NavigationManager Navigation { get; set; }

    [Parameter]
    [SupplyParameterFromQuery]
    public int? Year { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await ConnectHubs();
    }

    protected override async Task OnParametersSetAsync()
    {
        _selectedYear = Year ?? DateTime.Now.Year;
        await LoadCalendar(_selectedYear);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Year.HasValue)
            OnYearChange(Year.Value);

        await base.OnAfterRenderAsync(firstRender);
        _isRendered = true;
    }

    private async Task OnMergeClicked()
    {
        await Batch.ExecuteMerge();
    }

    private void OnYearChange(int year)
    {
        Navigation.NavigateTo($"import?tab=batch&year={year}");
    }

    private async Task LoadCalendar(int year)
    {
        _selectedYear = year;
        _inventory = await Inventory.GetInventoryByYearAsync(year);
        _batchDates = await Batch.GetExistsByYear(_selectedYear);
        _index = (await Inventory.GetInventoryAsync())
            .OrderByDescending(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);

        StateHasChanged();
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
            .Subscribe(_ => InvokeAsync(async () => await LoadCalendar(_selectedYear)));

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

        await _hubConnection.InvokeAsync("JoinGroup", Const.Hubs.Importer.Actions.Batch);
    }
}
