using Fip.Strive.Harvester.Application.Core.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace Fip.Strive.Harvester.Web.Components.Layout;

public partial class MainLayout
{
    private bool _isDarkMode = true;
    private MudThemeProvider _mudThemeProvider;

    MudTheme _striveTheme = new()
    {
        Typography = new Typography
        {
            H1 = new H1Typography { FontSize = "3rem" },
            H2 = new H2Typography { FontSize = "2rem" },
            H3 = new H3Typography { FontSize = "1.6rem" },
            H4 = new H4Typography { FontSize = "1.3rem" },
        },
    };

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isDarkMode = await _mudThemeProvider.GetSystemDarkModeAsync();
            await _mudThemeProvider.WatchSystemDarkModeAsync(OnSystemDarkModeChanged);
            StateHasChanged();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
            await _hubConnection.DisposeAsync();
    }

    private Task OnSystemDarkModeChanged(bool newValue)
    {
        _isDarkMode = newValue;
        StateHasChanged();
        return Task.CompletedTask;
    }
}
