using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class BatchTab : ComponentBase
{
    private HubConnection? hubConnection;
    private Dictionary<int, int[]> inventory = new();
    private bool isMergeActive;
    private MudTable<BatchIndex>? fileTable;

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public ApiService? Api { get; set; }

    [Inject]
    public BatchApi? BatchApi { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Api != null)
            inventory = (await Api.GetYearlyInventoryAsync())
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

        await ConnectHubs();

        await base.OnParametersSetAsync();
    }

    private async Task OnMergeClicked()
    {
        if (BatchApi == null)
            return;

        isMergeActive = true;
        await BatchApi.ExecuteMerge();
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
            result =>
            {
                isMergeActive = false;
                InvokeAsync(StateHasChanged);
            }
        );

        hubConnection.On<int>(
            Const.Hubs.Ingester.BatchProgress,
            result =>
            {
                InvokeAsync(StateHasChanged);
            }
        );

        await hubConnection.StartAsync();
    }

    private async Task<TableData<BatchIndex>> LoadServerData(TableState state, CancellationToken ct)
    {
        if (BatchApi == null)
            return new TableData<BatchIndex> { TotalItems = 0, Items = [] };

        var paged = new PagedRequest(state.Page, state.PageSize);
        var result = await BatchApi.GetFilesAsync(paged);

        return new TableData<BatchIndex> { TotalItems = result.Total, Items = result.Items };
    }
}
