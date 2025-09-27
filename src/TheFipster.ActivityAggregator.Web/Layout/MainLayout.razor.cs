using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace TheFipster.ActivityAggregator.Web.Layout;

public partial class MainLayout
{
    private int year = DateTime.Now.Year;
    private string pageName = "";

    private HubConnection? hubConnection;
    private List<string> events = new();

    [Inject]
    public NavigationManager Navigation { get; set; }

    [Inject]
    public ISnackbar Snackbar { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Navigation != null && Navigation.Uri.Contains("calendar/year"))
        {
            pageName = "yearlyCalendar";
            var url = Navigation.ToAbsoluteUri(Navigation.Uri);
            var yearValue = url.ToString().Split("/").Last();
            if (int.TryParse(yearValue, out int parsedYear))
                year = parsedYear;
        }
        else
        {
            pageName = "";
        }

        events.Add(year.ToString());

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/eventhub"))
            .Build();

        hubConnection.On<string>(
            "ReceiveEvent",
            (msg) =>
            {
                events.Add(msg);
                Snackbar.Add(msg, Severity.Success);
                // InvokeAsync(StateHasChanged);
            }
        );

        await hubConnection.StartAsync();

        await base.OnParametersSetAsync();
    }
}
