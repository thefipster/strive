using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;

namespace Fip.Strive.Indexing.Application.Repositories.Contracts;

public interface ITypeRepository : IIndexerV2<TypeIndexV2, string>
{
    Task<TypeIndexV2?> FindAsync(string hash);
    Task<TypeMeta?> GetInfoAsync(string hash);
}
