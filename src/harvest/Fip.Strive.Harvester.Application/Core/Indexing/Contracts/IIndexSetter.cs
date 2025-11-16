namespace Fip.Strive.Harvester.Application.Core.Indexing.Contracts;

public interface IIndexSetter<TIndex>
{
    Task SetHashAsync(TIndex entry);
    Task SetFileAsync(TIndex entry);
}

public interface IIndexSetter<THashIndex, TNameIndex>
{
    Task SetHashAsync(THashIndex entry);
    Task SetFileAsync(TNameIndex entry);
}
