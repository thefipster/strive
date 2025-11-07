using Fip.Strive.Queue.Application.Components;
using Fip.Strive.Queue.Application.Services;
using Fip.Strive.Queue.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Queue.Application.UnitTests;

public class QueuedHostedServiceTests
{
    [Fact]
    public void Test1()
    {
        var services = new ServiceCollection();
        services.AddScoped<TestQueueWorker>();
        var provider = services.BuildServiceProvider();

        var factory = new QueueWorkerFactory(provider);

        var config = Options.Create<QueueConfig>(new QueueConfig());
        var logger = NullLogger<QueuedHostedService>.Instance;

        var queue = new QueuedHostedService(config, logger, factory);
    }
}
