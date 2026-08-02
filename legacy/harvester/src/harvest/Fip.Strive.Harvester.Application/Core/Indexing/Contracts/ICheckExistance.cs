namespace Fip.Strive.Harvester.Application.Core.Indexing.Contracts;

public interface ICheckExistance
{
    Task<bool> HashExistsAsync(string hash);
}
