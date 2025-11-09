using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;

namespace Fip.Strive.Indexing.Application.Repositories.Contracts;

public interface IAssimilateRepository : IIndexerV2<AssimilateIndexV2, string>
{
    Task<AssimilateIndexV2?> FindAsync(string hash);
}
