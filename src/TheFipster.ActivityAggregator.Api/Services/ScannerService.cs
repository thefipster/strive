using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Services.Abstractions;

namespace TheFipster.ActivityAggregator.Api.Services;

public class ScannerService : IScannerService
{
    private readonly HubConnection connection;
    private readonly IScanner scanner;

    public ScannerService(IScanner scanner)
    {
        this.scanner = scanner;

        connection = new HubConnectionBuilder().WithUrl("https://localhost:7260/scanhub").Build();
        connection.StartAsync().Wait();
    }

    public async Task CheckImportAsync(ImporterIndex import, CancellationToken ct)
    {
        var files = Directory
            .EnumerateFiles(import.Output, "*", SearchOption.AllDirectories)
            .ToArray();

        var procCount = 0;
        var watch = new Stopwatch();
        watch.Start();

        foreach (var file in files)
        {
            await scanner.CheckAsync(file, import.Hash, ct);
            procCount++;

            if (watch.ElapsedMilliseconds > 1000)
            {
                await connection.InvokeAsync(
                    "Progress",
                    import.Hash,
                    procCount,
                    cancellationToken: ct
                );
                watch.Restart();
            }
        }
        watch.Stop();
        await connection.InvokeAsync("Finished", import.Hash, cancellationToken: ct);
    }
}
