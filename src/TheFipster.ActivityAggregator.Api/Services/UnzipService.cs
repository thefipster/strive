using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services;

public class UnzipService : IUnzipService
{
    private readonly HubConnection connection;
    private readonly IUnzipper unzip;
    private readonly IIndexer<ZipIndex> indexer;

    public UnzipService(IUnzipper unzip, IIndexer<ZipIndex> indexer)
    {
        this.unzip = unzip;
        this.indexer = indexer;

        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7260/hubs/ingest")
            .Build();
        connection.StartAsync().Wait();
    }

    public async Task<ZipIndex> ExtractAsync(
        string zipFilepath,
        string outputDirectory,
        CancellationToken ct
    )
    {
        var file = new FileInfo(zipFilepath);

        await connection.InvokeAsync(
            "WorkerStart",
            $"Unzipping {file.Name}.",
            cancellationToken: ct
        );

        var zipSize = file.Length;
        var hash = await file.HashXx3Async(ct);
        var outputName = file.Name.Replace(file.Extension, string.Empty);
        var outputPath = Path.Combine(outputDirectory, outputName);

        var index = indexer.GetById(hash);
        if (index != null)
        {
            if (zipFilepath != index.ZipPath)
                index.AlternateFiles.Add(zipFilepath);

            indexer.Set(index);

            await connection.InvokeAsync(
                "UnzipFinished",
                file.Name,
                "File is already indexed.",
                cancellationToken: ct
            );
            return index;
        }

        var stats = unzip.Extract(zipFilepath, outputPath, true);

        index = new ZipIndex
        {
            Hash = hash,
            OutputPath = outputPath,
            ZipPath = zipFilepath,
            FileCount = stats.FileCount,
            PackedSize = zipSize,
            UnpackedSize = stats.Size,
        };

        indexer.Set(index);

        await connection.InvokeAsync(
            "UnzipFinished",
            file.Name,
            "Indexing complete.",
            cancellationToken: ct
        );
        return index;
    }
}
