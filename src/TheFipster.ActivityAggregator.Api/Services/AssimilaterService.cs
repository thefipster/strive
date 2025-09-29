using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services;

public class AssimilaterService : IAssimilaterService
{
    private readonly IAssimilater assimilater;
    private readonly IIndexer<ScannerIndex> indexer;
    private readonly HubConnection connection;

    public AssimilaterService(IAssimilater assimilater, IIndexer<ScannerIndex> indexer)
    {
        this.assimilater = assimilater;
        this.indexer = indexer;

        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7260/hubs/assimilate")
            .Build();
        connection.StartAsync().Wait();
    }

    public async Task ConvergeImportAsync(ImporterIndex import, CancellationToken ct)
    {
        var indexes = indexer.GetFiltered(x =>
            x.OriginHash == import.Hash && x.Classification != null
        );

        var procCount = 0;
        var outCount = 0;
        var watch = new Stopwatch();
        watch.Start();

        foreach (var index in indexes)
        {
            var assimilation = await assimilater.StandardizeAsync(index, ct);

            procCount++;
            outCount += assimilation.Count;

            if (watch.ElapsedMilliseconds > 1000)
            {
                await connection.InvokeAsync(
                    "Progress",
                    import.Hash,
                    procCount,
                    outCount,
                    cancellationToken: ct
                );
                watch.Restart();
            }
        }
        watch.Stop();
        await connection.InvokeAsync("Finished", import.Hash, cancellationToken: ct);
    }
}
