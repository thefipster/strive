namespace Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;

public interface IJobDeleter
{
    public int DeleteBefore(DateTime dateTime);

    void Rebuild();
}
