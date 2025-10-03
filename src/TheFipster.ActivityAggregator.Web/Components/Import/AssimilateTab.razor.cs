using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class AssimilateTab : ComponentBase
{
    private HubConnection? hubConnection;
    private bool isAssimilationActive;
    private MudTable<ExtractorIndex>? fileTable;

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public AssimilateApi? Api { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await ConnectHubs();
        await base.OnParametersSetAsync();
    }

    private async Task OnAssimilateClicked()
    {
        if (Api == null)
            return;

        isAssimilationActive = true;
        await Api.ExecuteAssimilation();
    }

    private async Task ConnectHubs()
    {
        if (Navigation is null)
            return;

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri(Const.Hubs.Ingester.Url))
            .Build();

        hubConnection.On<string>(
            Const.Hubs.Ingester.AssimilationFinished,
            _ =>
            {
                isAssimilationActive = false;
                fileTable?.ReloadServerData();
                InvokeAsync(StateHasChanged);
            }
        );

        hubConnection.On<int>(
            Const.Hubs.Ingester.AssimilationProgress,
            _ =>
            {
                fileTable?.ReloadServerData();
                InvokeAsync(StateHasChanged);
            }
        );

        await hubConnection.StartAsync();
    }

    private async Task<TableData<ExtractorIndex>> LoadServerData(
        TableState state,
        CancellationToken ct
    )
    {
        if (Api == null)
            return new TableData<ExtractorIndex> { TotalItems = 0, Items = [] };

        var paged = new PagedRequest(state.Page, state.PageSize);
        var result = await Api.GetFilesAsync(paged);

        return new TableData<ExtractorIndex> { TotalItems = result.Total, Items = result.Items };
    }
}
