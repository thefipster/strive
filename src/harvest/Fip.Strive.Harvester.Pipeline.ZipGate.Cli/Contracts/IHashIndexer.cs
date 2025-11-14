using Fip.Strive.Harvester.Domain.Indexes;

namespace Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Contracts;

public interface IHashIndexer
{
    Task UpsertAsync(ZipIndexV2 entry);

    Task<ZipIndexV2?> GetAsync(string hash);
}
