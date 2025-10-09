using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Services.Contracts;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services;

public class UnzipService : IUnzipService
{
    private readonly HubConnection _connection;
    private readonly IUnzipper _unzip;
    private readonly IIndexer<ZipIndex> _indexer;
    private readonly IOptions<ApiConfig> _config;

    private string UnzipPath => _config.Value.UnzipDirectoryPath;

    public UnzipService(IUnzipper unzip, IIndexer<ZipIndex> indexer, IOptions<ApiConfig> config)
    {
        _unzip = unzip;
        _indexer = indexer;
        _config = config;

        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7260/hubs/ingest")
            .Build();
        _connection.StartAsync().Wait();
    }

    public async Task<ZipIndex> ExtractAsync(
        string zipFilepath,
        string outputDirectory,
        CancellationToken ct
    )
    {
        var file = new FileInfo(zipFilepath);

        await _connection.InvokeAsync(
            Const.Hubs.Ingester.WorkerInfo,
            $"Unzipping {file.Name}.",
            cancellationToken: ct
        );

        var zipSize = file.Length;
        var hash = await file.HashXx3Async(ct);
        var outputName = file.Name.Replace(file.Extension, string.Empty);
        var outputPath = Path.Combine(UnzipPath, outputName);

        var index = _indexer.GetById(hash);
        if (index != null)
        {
            if (zipFilepath != index.ZipPath)
                index.AlternateFiles.Add(zipFilepath);

            _indexer.Set(index);

            await _connection.InvokeAsync(
                Const.Hubs.Ingester.UnzipFinished,
                file.Name,
                "File is already indexed.",
                cancellationToken: ct
            );
            return index;
        }

        var stats = _unzip.Extract(zipFilepath, true);

        index = new ZipIndex
        {
            Hash = hash,
            OutputPath = outputPath,
            ZipPath = zipFilepath,
            FileCount = stats.FileCount,
            PackedSize = zipSize,
            UnpackedSize = stats.Size,
        };

        _indexer.Set(index);

        await _connection.InvokeAsync(
            Const.Hubs.Ingester.UnzipFinished,
            file.Name,
            "Indexing complete.",
            cancellationToken: ct
        );

        await _connection.InvokeAsync(
            Const.Hubs.Ingester.WorkerInfo,
            "Indexing complete.",
            cancellationToken: ct
        );
        return index;
    }
}
