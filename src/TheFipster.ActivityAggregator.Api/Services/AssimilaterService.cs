using System.Diagnostics;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services;

public class AssimilaterService : IAssimilaterService
{
    private readonly IAssimilater assimilater;
    private readonly IIndexer<ScannerIndex> indexer;

    public AssimilaterService(IAssimilater assimilater, IIndexer<ScannerIndex> indexer)
    {
        this.assimilater = assimilater;
        this.indexer = indexer;
    }

    public async Task ConvergeImportAsync(ImporterIndex import, CancellationToken ct)
    {
        var indexes = indexer.GetFiltered(x =>
            x.OriginHash == import.Hash && x.Classification != null
        );

        var procCount = 0;
        var watch = new Stopwatch();
        watch.Start();

        foreach (var index in indexes)
        {
            await assimilater.StandardizeAsync(index, ct);
            procCount++;

            if (watch.ElapsedMilliseconds > 1000)
            {
                // await connection.InvokeAsync(
                //     "Progress",
                //     import.Hash,
                //     procCount,
                //     cancellationToken: ct
                // );
                watch.Restart();
            }
        }
        watch.Stop();
        // await connection.InvokeAsync("Finished", import.Hash, cancellationToken: ct);
    }
}
