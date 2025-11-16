namespace Fip.Strive.Harvester.Application.Core.Indexing.Contracts;

public interface ISetHashIndex<TIndex>
{
    Task SetHashAsync(TIndex entry);
}
