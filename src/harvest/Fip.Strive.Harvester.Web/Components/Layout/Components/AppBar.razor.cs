using Fip.Strive.Harvester.Application.Core.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Fip.Strive.Harvester.Web.Components.Layout.Components;

public partial class AppBar
{
    private HubConnection? _hubConnection;

    private int _queueCount;
    private double _queueRate;

    [Inject]
    public required NavigationManager Navigation { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri($"/hubs/{QueueHub.HubName}"))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<int, int, double>(
            QueueHub.QueueReportMethodName,
            (count, _, rate) =>
            {
                _queueCount = count;
                _queueRate = rate;
                InvokeAsync(StateHasChanged);
            }
        );

        await _hubConnection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
            await _hubConnection.DisposeAsync();
    }
}
