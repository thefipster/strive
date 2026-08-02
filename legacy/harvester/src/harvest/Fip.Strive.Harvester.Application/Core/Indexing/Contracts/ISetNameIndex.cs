namespace Fip.Strive.Harvester.Application.Core.Indexing.Contracts;

public interface ISetNameIndex<TIndex>
{
    Task SetFileAsync(TIndex entry);
}
