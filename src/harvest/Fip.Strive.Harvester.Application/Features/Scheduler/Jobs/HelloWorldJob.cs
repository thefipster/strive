using Microsoft.Extensions.Logging;
using Quartz;

namespace Fip.Strive.Harvester.Application.Features.Scheduler.Jobs;

public class HelloWorldJob : IJob
{
    private readonly ILogger<HelloWorldJob> _logger;

    public HelloWorldJob(ILogger<HelloWorldJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Hello world at {Time}", DateTimeOffset.Now);
        return Task.CompletedTask;
    }
}
