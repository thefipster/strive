using TheFipster.ActivityAggregator.Domain.Models;

namespace TheFipster.ActivityAggregator.Storage.Abstractions.Activity;

public interface IUnifiedRecordWriter
{
    void Upsert(UnifiedRecord record);
}
