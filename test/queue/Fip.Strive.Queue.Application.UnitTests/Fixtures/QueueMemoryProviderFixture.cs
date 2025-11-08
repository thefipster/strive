using Fip.Strive.Queue.Application.Services;
using Fip.Strive.Queue.Application.Services.Contracts;
using Fip.Strive.Queue.Application.Tasks;
using Fip.Strive.Queue.Application.Tasks.Contracts;
using Fip.Strive.Queue.Application.UnitTests.Components;
using Fip.Strive.Queue.Domain.Models;
using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Memory.Contexts;
using Fip.Strive.Queue.Storage.Memory.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TaskFactory = Fip.Strive.Queue.Application.Services.TaskFactory;

namespace Fip.Strive.Queue.Application.UnitTests.Fixtures;

public class QueueMemoryProviderFixture : IDisposable
{
    public ServiceProvider Provider { get; }

    public IProcessingService Service => Provider.GetRequiredService<IProcessingService>();
    public IQueueService Queue => Provider.GetRequiredService<IQueueService>();
    public TestQueueWorker Worker =>
        Provider.GetRequiredService<IQueueWorker>() as TestQueueWorker
        ?? throw new Exception("There is no registered worker.");

    public QueueMemoryProviderFixture()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ILogger<HostedService>>(NullLogger<HostedService>.Instance);
        services.AddSingleton<ILogger<QueueRunner>>(NullLogger<QueueRunner>.Instance);
        services.AddSingleton<ILogger<QueueReporter>>(NullLogger<QueueReporter>.Instance);

        services.AddSingleton(Options.Create(new QueueConfig()));

        services.AddSingleton<MemoryQueueContext>();
        services.AddSingleton<IJobControl, MemoryJobControl>();

        services.AddSingleton<IQueueWorker, TestQueueWorker>();
        services.AddSingleton<ITaskFactory, TaskFactory>();

        services.AddSingleton<QueueMetrics>();

        services.AddSingleton<IQueueService, QueueService>();
        services.AddSingleton<IProcessingService, ProcessingService>();

        Provider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        Provider.Dispose();
    }
}
