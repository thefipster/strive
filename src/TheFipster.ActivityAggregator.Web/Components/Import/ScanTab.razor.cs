using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Components.Import.Scan;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class ScanTab : ComponentBase
{
    private bool isScanActive;
    private HubConnection? hubConnection;
    private ScanActions scanActions;
    private MudTable<FileIndex> fileTable;

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public ScanService? Scanner { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await ConnectHubs();
        await base.OnParametersSetAsync();
    }

    private async Task OnScanClicked()
    {
        if (Scanner == null)
            return;

        isScanActive = true;
        await Scanner.ExecuteFileScan();
    }

    private async Task ConnectHubs()
    {
        if (Navigation is null)
            return;

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri(Const.Hubs.Ingester.Url))
            .Build();

        hubConnection.On<string>(
            Const.Hubs.Ingester.FileScanFinished,
            async (result) =>
            {
                await InvokeAsync(() =>
                {
                    isScanActive = false;
                    fileTable.ReloadServerData();
                    StateHasChanged();
                });
            }
        );

        hubConnection.On<int>(
            Const.Hubs.Ingester.FileScanProgress,
            async (count) =>
            {
                await InvokeAsync(() =>
                {
                    fileTable.ReloadServerData();
                    StateHasChanged();
                });
            }
        );

        await hubConnection.StartAsync();
    }

    private async Task<TableData<FileIndex>> LoadServerData(TableState state, CancellationToken ct)
    {
        var sortBy = state.SortLabel;
        var descending = state.SortDirection == SortDirection.Descending;

        if (Scanner == null)
            return new TableData<FileIndex> { TotalItems = 0, Items = [] };

        var paged = new PagedRequest(state.Page, state.PageSize);
        var result = await Scanner.GetFilesAsync(paged);

        return new TableData<FileIndex> { TotalItems = result.Total, Items = result.Items };
    }
}
