namespace Fip.Strive.Queue.Application.Repositories.Contracts;

public interface IJobDeleter
{
    public int DeleteBefore(DateTime dateTime);

    void Rebuild();
}
