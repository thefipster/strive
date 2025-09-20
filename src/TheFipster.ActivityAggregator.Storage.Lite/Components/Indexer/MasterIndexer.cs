using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

public class MasterIndexer(
    IIndexer<ScanIndex> scanIndex,
    IIndexer<ClassificationIndex> classIndex,
    IIndexer<TransformIndex> transformIndex,
    IIndexer<BundleIndex> bundleIndexer,
    IIndexer<UnifyIndex> unifyIndex
) : IMasterIndexer
{
    public IEnumerable<MasterIndex> GetByDay(DateTime date)
    {
        var unifiedIndexes = unifyIndex.GetFiltered(x => x.Day == date.Date);
        var indexes = new List<MasterIndex>();

        foreach (var unifiedIndex in unifiedIndexes)
        {
            var bundleIndexes = bundleIndexer
                .GetFiltered(x =>
                    x.Timestamp == unifiedIndex.Timestamp && x.Kind == unifiedIndex.Kind
                )
                .ToList();
            if (bundleIndexes.Count == 0)
                throw new IndexingException(
                    GetType().Name,
                    $"Found no {nameof(BundleIndex)} for {nameof(UnifyIndex)} using {unifiedIndex.Timestamp:yy-MM-dd HH:mm} {unifiedIndex.Kind}."
                );

            if (bundleIndexes.Count > 1)
                throw new IndexingException(
                    GetType().Name,
                    $"Found multiple {nameof(BundleIndex)} for {nameof(UnifyIndex)} using {unifiedIndex.Timestamp:yy-MM-dd HH:mm} {unifiedIndex.Kind}."
                );

            var bundleIndex = bundleIndexes.First();

            var transformIndexes = new List<TransformIndex>();
            foreach (var bundleFile in bundleIndex.Extractions)
                transformIndexes.Add(GetTransformIndexForBundleFile(bundleFile));

            var ingesterIndexes = new List<IngesterIndex>();
            var inputFiles = transformIndexes.Select(x => x.SourceFilepath).Distinct().ToList();
            foreach (var inputFile in inputFiles)
            {
                var classification = GetClassificationForSourceFile(inputFile);
                var scan = GetScanForSourceFile(inputFile);

                var ingestation = new IngesterIndex(inputFile, scan, classification);
                ingesterIndexes.Add(ingestation);
            }

            var masterIndex = new MasterIndex(
                unifiedIndex,
                bundleIndex,
                transformIndexes,
                ingesterIndexes
            );
            indexes.Add(masterIndex);
        }

        return indexes;
    }

    private ScanIndex GetScanForSourceFile(string sourceFile)
    {
        var indexes = scanIndex.GetFiltered(x => x.Filepath == sourceFile).ToList();

        if (indexes.Count == 1)
            return indexes.First();

        throw new IndexingException(
            GetType().Name,
            $"Found no {nameof(ScanIndex)} for {nameof(BundleIndex)} using {sourceFile}."
        );
    }

    private ClassificationIndex GetClassificationForSourceFile(string sourceFile)
    {
        var indexes = classIndex.GetFiltered(x => x.Filepath == sourceFile).ToList();

        if (indexes.Count == 1)
            return indexes.First();

        throw new IndexingException(
            GetType().Name,
            $"Found no {nameof(ClassificationIndex)} for {nameof(BundleIndex)} using {sourceFile}."
        );
    }

    private TransformIndex GetTransformIndexForBundleFile(string bundleFile)
    {
        var indexes = transformIndex.GetFiltered(x => x.Filepath == bundleFile).ToList();

        if (indexes.Count == 1)
            return indexes.First();

        throw new IndexingException(
            GetType().Name,
            $"Found no {nameof(TransformIndex)} for {nameof(BundleIndex)} using {bundleFile}."
        );
    }
}
