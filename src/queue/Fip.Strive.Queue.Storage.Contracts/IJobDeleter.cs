namespace Fip.Strive.Queue.Storage.Contracts;

public interface IJobDeleter
{
    public int DeleteBefore(DateTime dateTime);
}
