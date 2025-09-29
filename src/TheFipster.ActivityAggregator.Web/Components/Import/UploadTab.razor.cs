using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class UploadTab : ComponentBase
{
    private string uploadKey = Guid.NewGuid().ToString();
    private HubConnection? hubConnection;
    private IEnumerable<ZipIndex> zips = [];
    private bool isLoading = true;

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public UploadService? UploadService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (UploadService != null)
            zips = (await UploadService.GetZipsAsync()).OrderByDescending(x => x.IndexedAt);

        await ConnectHubs();
        await base.OnParametersSetAsync();
        isLoading = false;
    }

    private void OnUploadFinished()
    {
        uploadKey = Guid.NewGuid().ToString();
    }

    private async Task ConnectHubs()
    {
        if (Navigation is null)
            return;

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/hubs/ingest"))
            .Build();

        hubConnection.On<string, string>(
            "OnUnzipFinished",
            (file, result) =>
            {
                InvokeAsync(async () =>
                {
                    if (UploadService != null)
                        zips = await UploadService.GetZipsAsync();

                    StateHasChanged();
                });
            }
        );

        await hubConnection.StartAsync();
    }
}
