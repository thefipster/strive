using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class BatchTab : ComponentBase
{
    private HubConnection? _hubConnection;

    private bool _isRendered;
    private bool _isMergeActive;

    private int _selectedYear = DateTime.Now.Year;

    private Dictionary<int, int[]> _index = new();
    private IEnumerable<InventoryIndex> _inventory = [];
    private IEnumerable<DateTime>? _batchDates;

    private double _progress;
    private string? _progressMessage;

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public InventoryApi? Inventory { get; set; }

    [Inject]
    public BatchApi? Batch { get; set; }

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
        await LoadIndex();
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
            .WithUrl("https://localhost:7098" + Const.Hubs.Importer.Url)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string, bool>(
            Const.Hubs.Importer.ReportAction,
            (_, updated) =>
            {
                if (!updated)
                    return;

                _progress = 0;
                _isMergeActive = false;
                _progressMessage = null;

                InvokeAsync(async () =>
                {
                    await LoadIndex();
                    StateHasChanged();
                });
            }
        );

        _hubConnection.On<string, double>(
            Const.Hubs.Importer.ReportProgress,
            (message, progress) =>
            {
                _progress = progress;
                _progressMessage = message;

                InvokeAsync(async () =>
                {
                    await LoadIndex();
                    StateHasChanged();
                });
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

        await _hubConnection.InvokeAsync("JoinGroup", Const.Hubs.Importer.Actions.Batch);
    }
}
