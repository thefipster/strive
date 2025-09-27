using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace TheFipster.ActivityAggregator.Web.Layout;

public partial class MainLayout
{
    private int year = DateTime.Now.Year;
    private string pageName = "";
    private bool drawerOpen;

    private HubConnection? hubConnection;

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public ISnackbar? Snackbar { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        AppendCalendarNavigation();
        await ConnectHubs();
        await base.OnParametersSetAsync();
    }

    private void OnDrawerToggle() => drawerOpen = !drawerOpen;

    private async Task ConnectHubs()
    {
        if (Navigation is null)
            return;

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/eventhub"))
            .Build();

        hubConnection.On<string>(
            "ReceiveEvent",
            (msg) =>
            {
                if (Snackbar != null)
                    Snackbar.Add(msg, Severity.Success);

                InvokeAsync(StateHasChanged);
            }
        );

        await hubConnection.StartAsync();
    }

    private void AppendCalendarNavigation()
    {
        pageName = "";
        if (Navigation != null && Navigation.Uri.Contains("calendar/year"))
            ConfigureYearNavigation(Navigation);
    }

    private void ConfigureYearNavigation(NavigationManager nav)
    {
        pageName = "yearlyCalendar";
        var url = nav.ToAbsoluteUri(nav.Uri);
        var yearValue = url.ToString().Split("/").Last();
        if (int.TryParse(yearValue, out int parsedYear))
            year = parsedYear;
    }
}
