namespace Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;

public interface IQueueReporter
{
    Task RunAsync(CancellationToken ct);
}
