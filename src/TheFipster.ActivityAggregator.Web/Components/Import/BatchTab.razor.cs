using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class BatchTab : ComponentBase
{
    private HubConnection? _hubConnection;
    private bool _isMergeActive;
    private int _selectedYear = DateTime.Now.Year;
    private Dictionary<int, int[]> _index = new();
    private IEnumerable<InventoryIndex> _inventory = [];
    private bool _isRendered;
    private IEnumerable<DateTime>? _batchDates;

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public InventoryApi? Inventory { get; set; }

    [Inject]
    public BatchApi? Batch { get; set; }

    [Parameter]
    [SupplyParameterFromQuery]
    public int? Year { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await LoadIndex();
        await ConnectHubs();
        await OnYearChange(_selectedYear);
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
        if (Batch == null)
            return;

        _isMergeActive = true;
        await Batch.ExecuteMerge();
    }

    private async Task OnYearChange(int year) => await LoadCalendar(year);

    private async Task LoadCalendar(int year)
    {
        _selectedYear = year;
        if (Inventory != null)
            _inventory = await Inventory.GetInventoryByYearAsync(year);

        if (Batch != null)
            _batchDates = await Batch.GetExistsByYear(_selectedYear);

        StateHasChanged();
    }

    private async Task LoadIndex()
    {
        if (Inventory != null)
            _index = (await Inventory.GetInventoryAsync())
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);
    }

    private async Task ConnectHubs()
    {
        if (Navigation is null)
            return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri(Const.Hubs.Ingester.Url))
            .Build();

        _hubConnection.On<string>(
            Const.Hubs.Ingester.BatchFinished,
            _ =>
            {
                _isMergeActive = false;
                InvokeAsync(async () =>
                {
                    await LoadIndex();
                    StateHasChanged();
                });
            }
        );

        _hubConnection.On<int>(
            Const.Hubs.Ingester.BatchProgress,
            _ =>
            {
                InvokeAsync(async () =>
                {
                    await LoadIndex();
                    StateHasChanged();
                });
            }
        );

        await _hubConnection.StartAsync();
    }
}
