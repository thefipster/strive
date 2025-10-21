using Fip.Strive.Harvester.Application.Core.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace Fip.Strive.Harvester.Web.Components.Layout;

public partial class MainLayout
{
    private HubConnection? _hubConnection;

    [Inject]
    public required NavigationManager Navigation { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri($"/hubs/{HelloWorldHub.HubName}"))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string>(
            "HelloWorld",
            msg =>
            {
                Snackbar.Add(msg, Severity.Info);
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
