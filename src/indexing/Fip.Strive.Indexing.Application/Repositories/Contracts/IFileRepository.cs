using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;

namespace Fip.Strive.Indexing.Application.Repositories.Contracts;

public interface IFileRepository : IIndexerV2<FileIndexV2, string>
{
    Task<FileMeta?> GetInfoAsync(string hash);
}
