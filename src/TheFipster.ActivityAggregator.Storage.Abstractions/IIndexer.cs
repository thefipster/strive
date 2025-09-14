namespace TheFipster.ActivityAggregator.Storage.Abstractions;

public interface IIndexer<TIndex, TId, TFilter>
{
    void Set(TIndex index);

    TIndex? Get(TId id);

    IEnumerable<TIndex> Filter(TFilter filter);
}
