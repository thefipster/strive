namespace Fip.Strive.Queue.Application.Components.Contracts;

public interface IQueueRunner
{
    public bool IsRunning { get; }
    Task RunAsync(int workerId, CancellationToken ct);
}
