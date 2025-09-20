using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class MasterIndex(
    UnifyIndex unifiedIndex,
    BundleIndex bundleIndex,
    List<TransformIndex> transformIndexes,
    List<IngesterIndex> ingesterIndexes
)
{
    public DateTime Timestamp { get; } = bundleIndex.Timestamp;
    public DataKind Kind { get; } = bundleIndex.Kind;
    public bool HasConflicts { get; } = unifiedIndex.HasConflicts;

    public List<IngesterIndex> Imports { get; } = ingesterIndexes;
    public List<TransformIndex> Transformations { get; } = transformIndexes;

    public int BundleVersion { get; } = bundleIndex.Version;
    public DateTime BundledAt { get; } = bundleIndex.IndexedAt;

    public int UnifierVersion { get; } = unifiedIndex.Version;
    public DateTime UnifiedAt { get; } = unifiedIndex.IndexedAt;
}
