using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class ScanTab : ComponentBase
{
    private bool _isScanActive;
    private HubConnection? _hubConnection;
    private MudTable<FileIndex>? _fileTable;

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public ScanApi? Scanner { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await ConnectHubs();
        await base.OnParametersSetAsync();
    }

    private async Task OnScanClicked()
    {
        if (Scanner == null)
            return;

        _isScanActive = true;
        await Scanner.ExecuteFileScan();
    }

    private async Task ConnectHubs()
    {
        if (Navigation is null)
            return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri(Const.Hubs.Ingester.Url))
            .Build();

        _hubConnection.On<string>(
            Const.Hubs.Ingester.FileScanFinished,
            async (_) =>
            {
                await InvokeAsync(() =>
                {
                    _isScanActive = false;
                    _fileTable?.ReloadServerData();
                    StateHasChanged();
                });
            }
        );

        _hubConnection.On<int>(
            Const.Hubs.Ingester.FileScanProgress,
            async _ =>
            {
                await InvokeAsync(() =>
                {
                    _fileTable?.ReloadServerData();
                    StateHasChanged();
                });
            }
        );

        await _hubConnection.StartAsync();
    }

    private async Task<TableData<FileIndex>> LoadServerData(TableState state, CancellationToken ct)
    {
        if (Scanner == null)
            return new TableData<FileIndex> { TotalItems = 0, Items = [] };

        var paged = new PagedRequest(state.Page, state.PageSize);
        var result = await Scanner.GetFilesAsync(paged);

        return new TableData<FileIndex> { TotalItems = result.Total, Items = result.Items };
    }
}
