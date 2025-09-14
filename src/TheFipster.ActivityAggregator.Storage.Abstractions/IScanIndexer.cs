using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Storage.Abstractions;

public interface IScanIndexer : IIndexer<ClassificationIndex, string, DataSources> { }
