using Fip.Strive.Harvester.Domain.Indexes;

namespace Fip.Strive.Harvester.Application.Core.Indexing.Contracts;

public interface IIndexPuller
{
    Task<ZipIndex?> GetFileAsync(ZipIndex entry);
}
