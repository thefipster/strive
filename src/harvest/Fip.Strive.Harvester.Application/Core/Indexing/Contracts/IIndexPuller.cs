using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;

namespace Fip.Strive.Harvester.Application.Core.Indexing.Contracts;

public interface IIndexPuller
{
    Task<ZipIndex?> GetFileAsync(ZipIndex entry);
}
