using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using TheFipster.ActivityAggregator.Domain;
using Defaults = TheFipster.ActivityAggregator.Domain.Defaults;

namespace TheFipster.ActivityAggregator.Web.Layout;

public partial class MainLayout
{
    private int _year = DateTime.Now.Year;
    private string _pageName = "";
    private bool _drawerOpen;

    private HubConnection? _hubConnection;

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

    private void OnDrawerToggle() => _drawerOpen = !_drawerOpen;

    private async Task ConnectHubs()
    {
        if (Navigation is null)
            return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7098" + Defaults.Hubs.Importer.Url)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string, bool>(
            Defaults.Hubs.Importer.ReportAction,
            (msg, _) =>
            {
                Snackbar?.Add(msg, Severity.Info);
                InvokeAsync(StateHasChanged);
            }
        );

        await _hubConnection.StartAsync();
        await JoinGroups();
        _hubConnection.Reconnected += async _ => await JoinGroups();
    }

    private async Task JoinGroups()
    {
        if (_hubConnection == null)
            return;

        await _hubConnection.InvokeAsync("JoinGroup", Defaults.Hubs.Importer.Actions.Unzip);
        await _hubConnection.InvokeAsync("JoinGroup", Defaults.Hubs.Importer.Actions.Scan);
        await _hubConnection.InvokeAsync("JoinGroup", Defaults.Hubs.Importer.Actions.Assimilate);
    }

    private void AppendCalendarNavigation()
    {
        _pageName = "";
        if (Navigation != null && Navigation.Uri.Contains("calendar/year"))
            ConfigureYearNavigation(Navigation);
    }

    private void ConfigureYearNavigation(NavigationManager nav)
    {
        _pageName = "yearlyCalendar";
        var url = nav.ToAbsoluteUri(nav.Uri);
        var yearValue = url.ToString().Split("/").Last();
        if (int.TryParse(yearValue, out int parsedYear))
            _year = parsedYear;
    }
}
