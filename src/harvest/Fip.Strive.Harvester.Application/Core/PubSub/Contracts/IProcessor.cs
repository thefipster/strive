namespace Fip.Strive.Harvester.Application.Core.PubSub.Contracts;

public interface IProcessor
{
    Task ProcessAsync(string message, CancellationToken cancellationToken);
}
