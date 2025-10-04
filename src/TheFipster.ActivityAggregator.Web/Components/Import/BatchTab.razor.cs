using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class BatchTab : ComponentBase
{
    private HubConnection? hubConnection;
    private bool isMergeActive;
    private int selectedYear = DateTime.Now.Year;
    private Dictionary<int, int[]> index = new();
    private IEnumerable<InventoryIndex> inventory = [];
    private bool _isRendered;

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
        await OnYearChange(selectedYear);
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

        isMergeActive = true;
        await Batch.ExecuteMerge();
    }

    private async Task OnYearChange(int year) => await LoadCalendar(year);

    private async Task LoadCalendar(int year)
    {
        selectedYear = year;
        if (Inventory is null)
            return;

        inventory = await Inventory.GetInventoryByYearAsync(year);
        StateHasChanged();
    }

    private async Task LoadIndex()
    {
        if (Inventory != null)
            index = (await Inventory.GetInventoryAsync())
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);
    }

    private async Task ConnectHubs()
    {
        if (Navigation is null)
            return;

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri(Const.Hubs.Ingester.Url))
            .Build();

        hubConnection.On<string>(
            Const.Hubs.Ingester.BatchFinished,
            _ =>
            {
                isMergeActive = false;
                InvokeAsync(async () =>
                {
                    await LoadIndex();
                    StateHasChanged();
                });
            }
        );

        hubConnection.On<int>(
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

        await hubConnection.StartAsync();
    }
}
