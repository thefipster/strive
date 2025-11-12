using Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Pipeline.ZipGate.Cli;

public class Worker(ILogger<Worker> logger) : IMessageProcessor
{
    public Task ProcessAsync(string message, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Worker received message: {message}");
        return Task.CompletedTask;
    }
}
