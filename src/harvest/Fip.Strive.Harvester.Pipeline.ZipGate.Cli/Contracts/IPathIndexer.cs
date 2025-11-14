using Fip.Strive.Harvester.Domain.Indexes;

namespace Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Contracts;

public interface IPathIndexer
{
    Task UpsertAsync(ZipIndexV2 entry);
}
