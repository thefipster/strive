namespace Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Contracts;

public interface IMessageProcessor
{
    Task ProcessAsync(string message, CancellationToken cancellationToken);
}
