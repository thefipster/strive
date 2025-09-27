using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class ImportScan : ComponentBase
{
    private IEnumerable<ImporterIndex> indexes = [];
    private Dictionary<string, int> indexProgress = new();
    private Dictionary<string, bool> indexLocked = new();
    private HubConnection? hubConnection;
    private bool indexLoading = true;

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public ApiService? Api { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await UpdateUploadsAsync();
        await ConnectHubs();

        await base.OnParametersSetAsync();
    }

    private async Task OnSyncClickedAsync()
    {
        await UpdateUploadsAsync();
    }

    private async Task UpdateUploadsAsync()
    {
        if (Api != null)
        {
            indexes = (await Api.GetImporterIndexesAsync()).OrderByDescending(x => x.IndexedAt);
            foreach (var index in indexes)
            {
                indexProgress.Add(index.Hash, 0);
                indexLocked.Add(index.Hash, false);
            }

            indexLoading = false;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task ConnectHubs()
    {
        if (Navigation is null)
            return;

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/scanhub"))
            .Build();

        hubConnection.On<string>(
            "OnFinished",
            hash =>
            {
                var index = indexes.FirstOrDefault(x => x.Hash == hash);
                if (index != null)
                    indexProgress[hash] = index.Count;

                indexLocked[hash] = false;

                InvokeAsync(StateHasChanged);
            }
        );

        hubConnection.On<string, int>(
            "OnProgress",
            (hash, procCount) =>
            {
                indexLocked[hash] = true;
                indexProgress[hash] = procCount;
                InvokeAsync(StateHasChanged);
            }
        );

        await hubConnection.StartAsync();
    }

    private async Task OnScanClicked(string indexHash)
    {
        indexLocked[indexHash] = true;
        if (Api != null)
            await Api.Scan(indexHash);
    }
}
