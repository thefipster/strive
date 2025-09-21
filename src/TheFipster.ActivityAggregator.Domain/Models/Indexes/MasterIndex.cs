using System.Text.Json.Serialization;
using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class MasterIndex
{
    public MasterIndex() { }

    public MasterIndex(
        UnifyIndex unifiedIndex,
        BundleIndex bundleIndex,
        List<TransformIndex> transformIndexes,
        List<IngesterIndex> ingesterIndexes
    )
    {
        Timestamp = bundleIndex.Timestamp;
        Kind = bundleIndex.Kind;
        HasConflicts = unifiedIndex.HasConflicts;
        Imports = ingesterIndexes;
        Transformations = transformIndexes;
        BundlerVersion = bundleIndex.Version;
        BundledAt = bundleIndex.IndexedAt;
        UnifierVersion = unifiedIndex.Version;
        UnifiedAt = unifiedIndex.IndexedAt;
    }

    public DateTime Timestamp { get; set; }

    public DataKind Kind { get; set; }

    public bool HasConflicts { get; set; }

    public List<IngesterIndex> Imports { get; set; } = new();

    public List<TransformIndex> Transformations { get; set; } = new();

    public int BundlerVersion { get; set; }

    public DateTime BundledAt { get; set; }

    public int UnifierVersion { get; set; }

    public DateTime UnifiedAt { get; set; }
}
