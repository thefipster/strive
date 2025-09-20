using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

public class ClassificationIndexer(IndexerContext context)
    : BaseIndexer<ClassificationIndex>(context);
