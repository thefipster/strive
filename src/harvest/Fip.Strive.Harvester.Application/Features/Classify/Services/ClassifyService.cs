using Fip.Strive.Harvester.Application.Features.Classify.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Indexing.Application.Repositories.Contracts;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Ingestion.Application.Services.Contracts;
using Fip.Strive.Queue.Application.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Classify.Services;

public class ClassifyService(
    IFileRepository files,
    ITypeRepository types,
    IClassificationService classificationService,
    IQueueService queue
) : IClassifyService
{
    public async Task ScanFile(FileSignal signal, CancellationToken ct)
    {
        var info = await files.GetInfoAsync(signal.Hash);

        if (info == null || FileIsNew(signal, info) || ClassifiersHaveChanged(info))
            await CreateFileIndex(signal);

        if (info != null && ClassifiersAreTheSame(info))
            return;

        var index = await ExecuteClassification(signal, ct);

        if (index == null)
            return;

        await EnqueueSignalAsync(index);
    }

    private async Task EnqueueSignalAsync(TypeIndexV2 index)
    {
        var signal = new TypedSignal
        {
            Hash = index.Hash,
            Source = index.Source,
            Filepath = index.Filepath,
        };

        await queue.EnqueueAsync(signal);
    }

    private async Task<TypeIndexV2?> ExecuteClassification(FileSignal signal, CancellationToken ct)
    {
        var index = await types.FindAsync(signal.Hash);
        if (index != null && ClassificationIsUpToDate(index))
            return null;

        var result = classificationService.Classify(signal.Filepath, ct);
        var successes = result.Where(x => x.Classification != null).ToList();

        if (successes.Count != 1)
            return null;

        var classification = successes.First();
        index = new TypeIndexV2
        {
            Filepath = signal.Filepath,
            Hash = signal.Hash,
            Source = classification.Source,
            Version = classification.Version,
        };

        await types.SetAsync(index);
        return index;
    }

    private bool ClassificationIsUpToDate(TypeIndexV2 index)
    {
        var classifier = classificationService.Find(index.Source);
        if (classifier == null)
            throw new InvalidOperationException(
                $"No classifier found for source {index.Source} even though it is a known hash {index.Hash}."
            );

        if (classifier.ClassifierVersion <= index.Version)
            return true;

        return false;
    }

    private async Task CreateFileIndex(FileSignal signal)
    {
        var fileHash = new FileIndexV2
        {
            Hash = signal.Hash,
            ClassificationHash = classificationService.GetHash(),
            Filepath = signal.Filepath,
            ParentFilepath = signal.ParentFilepath,
        };

        await files.SetAsync(fileHash);
    }

    private bool ClassifiersHaveChanged(FileMeta info)
    {
        var classificationHash = classificationService.GetHash();
        return !info.ClassificationHashes.Contains(classificationHash);
    }

    private bool ClassifiersAreTheSame(FileMeta info) => !ClassifiersHaveChanged(info);

    private bool FileIsNew(FileSignal signal, FileMeta info) =>
        info.Files.Contains(signal.Filepath);
}
