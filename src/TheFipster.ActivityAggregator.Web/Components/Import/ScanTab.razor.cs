using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class ScanTab : ComponentBase
{
    private bool actionDisabled;
    private HubConnection? hubConnection;

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

        actionDisabled = true;
        await Scanner.ExecuteFileScan();
    }

    private async Task ConnectHubs()
    {
        if (Navigation is null)
            return;

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri(Const.Hubs.Ingester.Url))
            .Build();

        hubConnection.On<string, string>(
            Const.Hubs.Ingester.FileScanFinished,
            async (file, result) =>
            {
                actionDisabled = false;
                await InvokeAsync(StateHasChanged);
            }
        );

        await hubConnection.StartAsync();
    }
}
