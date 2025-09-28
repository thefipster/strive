using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import.Assimilate;

public partial class ImportAssimilation : ComponentBase
{
    private IEnumerable<ImporterIndex> indexes = [];
    private readonly Dictionary<string, int> indexClassified = new();
    private readonly Dictionary<string, int> indexUnclassified = new();
    private readonly Dictionary<string, int> indexConverged = new();
    private readonly Dictionary<string, int> indexOutput = new();
    private readonly Dictionary<string, int> indexHashes = new();
    private readonly Dictionary<string, bool> indexLocked = new();
    private HubConnection? hubConnection;
    private bool indexLoading = true;

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public ApiService? Api { get; set; }

    [Parameter]
    public EventCallback<string> HashSelected { get; set; }

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

    private async Task OnScanClicked(string indexHash)
    {
        indexLocked[indexHash] = true;
        if (Api != null)
            await Api.Assimilate(indexHash);
    }

    private async Task OnAssimilateRowClicked(TableRowClickEventArgs<ImporterIndex> obj)
    {
        await HashSelected.InvokeAsync(obj.Item?.Hash);
    }

    private async Task UpdateUploadsAsync()
    {
        if (Api != null)
        {
            indexes = (await Api.GetImporterIndexesAsync()).OrderByDescending(x => x.IndexedAt);
            foreach (var index in indexes)
            {
                var scanCount = await Api.GetScannerIndexCountAsync(index.Hash);
                var assimilateCount = await Api.GetAssimilateIndexCountAsync(index.Hash);

                indexHashes[index.Hash] = scanCount[0];
                indexClassified[index.Hash] = scanCount[2];
                indexUnclassified[index.Hash] = scanCount[3];
                indexConverged[index.Hash] = assimilateCount[0];
                indexOutput[index.Hash] = assimilateCount[1];
                indexLocked[index.Hash] = false;
                indexLocked[index.Hash] = false;
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
            .WithUrl(Navigation.ToAbsoluteUri("/hubs/assimilate"))
            .Build();

        hubConnection.On<string>(
            "OnFinished",
            hash =>
            {
                var index = indexes.FirstOrDefault(x => x.Hash == hash);
                if (index != null)
                    indexClassified[hash] = index.Count;

                indexLocked[hash] = false;

                InvokeAsync(StateHasChanged);
            }
        );

        hubConnection.On<string, int, int>(
            "OnProgress",
            (hash, procCount, outCount) =>
            {
                indexLocked[hash] = true;
                indexConverged[hash] = procCount;
                indexOutput[hash] = outCount;
                InvokeAsync(StateHasChanged);
            }
        );

        await hubConnection.StartAsync();
    }
}
