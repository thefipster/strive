using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

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

    [Parameter]
    [SupplyParameterFromQuery]
    public int? Year { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await ConnectHubs();
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await Update(_selectedYear);
        await base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Year.HasValue)
            await OnYearChange(Year.Value);

        await base.OnAfterRenderAsync(firstRender);
        _isRendered = true;
    }

    private async Task OnMergeClicked()
    {
        await Batch.ExecuteMerge();
    }

    private async Task OnYearChange(int year) => await LoadCalendar(year);

    private async Task Update(int year)
    {
        await LoadIndex();
        await OnYearChange(year);
    }

    private async Task LoadCalendar(int year)
    {
        _selectedYear = year;
        _inventory = await Inventory.GetInventoryByYearAsync(year);
        _batchDates = await Batch.GetExistsByYear(_selectedYear);

        StateHasChanged();
    }

    private async Task LoadIndex()
    {
        _index = (await Inventory.GetInventoryAsync())
            .OrderByDescending(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);
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
                InvokeAsync(async () =>
                {
                    await Update(_selectedYear);
                    StateHasChanged();
                });
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

        await _hubConnection.InvokeAsync("JoinGroup", Const.Hubs.Importer.Actions.Batch);
    }
}
