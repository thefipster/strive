using HashifyNet.Algorithms.RapidHash;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Services.Worker;

public class Extractor(IUnzipper unzip, IIndexer<ImporterIndex> indexer) : IExtractor
{
    public async Task<ImporterIndex> ReadAsync(
        string zipFilepath,
        string outputDirectory,
        CancellationToken ct
    )
    {
        var file = new FileInfo(zipFilepath);
        var zipSize = file.Length;
        var hash = await file.HashXx3Async(ct);
        var outputName = file.Name.Replace(file.Extension, string.Empty);
        var outputPath = Path.Combine(outputDirectory, outputName);

        var index = indexer.GetById(hash);
        if (IndexIsAlreadyPublished(index, file))
            return Updated(index!);

        unzip.Extract(zipFilepath, outputPath, true);
        (var count, var size) = GetDirectoryStats(outputPath);

        index = new ImporterIndex { Hash = hash, Output = outputPath };
        index.Files.Add(file.FullName);
        index.Count = count;
        index.PackedSize = zipSize;
        index.UnpackedSize = size;
        index.Actions.Log(ImporterActions.Extracted);
        return Updated(index);
    }

    private bool IndexIsAlreadyPublished(ImporterIndex? index, FileInfo file)
    {
        if (index == null)
            return false;

        if (!index.Files.Contains(file.FullName))
            index.Files.Add(file.FullName);

        index.Actions.Log(ImporterActions.NoOperation);
        return true;
    }

    private ImporterIndex Updated(ImporterIndex index)
    {
        indexer.Set(index);
        return index;
    }

    public (int fileCount, long totalSize) GetDirectoryStats(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        int fileCount = 0;
        long totalSize = 0;

        void ProcessDirectory(string path)
        {
            var files = Directory.GetFiles(path);
            fileCount += files.Length;

            foreach (var file in files)
            {
                totalSize += new FileInfo(file).Length;
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                ProcessDirectory(dir);
            }
        }

        ProcessDirectory(directoryPath);

        return (fileCount, totalSize);
    }
}
