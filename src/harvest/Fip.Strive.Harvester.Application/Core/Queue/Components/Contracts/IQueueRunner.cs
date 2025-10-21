namespace Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;

public interface IQueueRunner
{
    Task RunAsync(int workerId, CancellationToken ct);
}
