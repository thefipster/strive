using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Core.Ingestion.Services.Contracts;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Classify.Models;
using Fip.Strive.Harvester.Application.Features.Classify.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Classify.Services;

public class ClassificationService(
    IIndexer<FileIndex, string> indexer,
    IClassifier classifier,
    ISignalQueue queue
) : IScanner
{
    public Task<WorkItem> ClassifyAsync(FileSignal signal, CancellationToken ct)
    {
        var work = WorkItem.FromSignal(signal);
        work.Classifications = classifier.Classify(work.Signal.Filepath, ct);

        return ReportClassification(work, ct);
    }

    private Task<WorkItem> ReportClassification(WorkItem work, CancellationToken ct = default)
    {
        work.Index = GetIndex(work);
        AppendBasicClassificationInfo(work.Index);

        if (UniqueClassification(work))
        {
            AppendClassificationResult(work);
            EmitSignal(work, ct);
        }

        if (NoClassification(work))
            work.Index.ClassificationResult = ClassificationResults.Unclassified;

        if (MultipleClassifications(work))
            work.Index.ClassificationResult = ClassificationResults.Overclassified;

        indexer.Upsert(work.Index);

        return Task.FromResult(work);
    }

    private void EmitSignal(WorkItem work, CancellationToken ct)
    {
        var signal = work.ToSignal();
        queue.EnqueueAsync(signal, ct);
    }

    private static void AppendClassificationResult(WorkItem work)
    {
        var index = work.Index!;

        index.Classified = true;
        index.ClassificationResult = ClassificationResults.Classified;

        var classification = work
            .Classifications.Where(x => x.Classification != null)
            .Select(x => x.Classification!)
            .First();
        index.Source = classification.Source;
        index.Timestamp = classification.Datetime;
        index.ClassfierVersion = classification.Version;
    }

    private FileIndex GetIndex(WorkItem work)
    {
        var index = indexer.Find(work.Signal.Hash);
        if (index == null)
            throw new InvalidOperationException("Index not found.");

        return index;
    }

    private void AppendBasicClassificationInfo(FileIndex index)
    {
        index.ClassifierHash = classifier.GetHash();
        index.LastClassificationAt = DateTime.UtcNow;
        index.Classified = false;
    }

    private static bool MultipleClassifications(WorkItem work) =>
        work.Classifications.Count(x => x.Classification != null) > 1;

    private static bool NoClassification(WorkItem work) =>
        work.Classifications.Count(x => x.Classification != null) == 0;

    private static bool UniqueClassification(WorkItem work) =>
        work.Classifications.Count(x => x.Classification != null) == 1;
}
