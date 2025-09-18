namespace TheFipster.ActivityAggregator.Storage.Abstractions;

public interface IIndexer<TIndex>
{
    void Set(TIndex index);

    TIndex? GetById(string id);

    IEnumerable<TIndex> GetFiltered(string filter);

    IEnumerable<TIndex> GetAll();
}
