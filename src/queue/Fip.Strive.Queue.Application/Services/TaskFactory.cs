using Fip.Strive.Queue.Application.Services.Contracts;
using Fip.Strive.Queue.Application.Tasks;
using Fip.Strive.Queue.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Queue.Application.Services;

public class TaskFactory(IServiceProvider serviceProvider, QueueMetrics metrics) : ITaskFactory
{
    public QueueRunner CreateRunner() =>
        ActivatorUtilities.CreateInstance<QueueRunner>(serviceProvider, metrics);

    public QueueReporter CreateReporter() =>
        ActivatorUtilities.CreateInstance<QueueReporter>(serviceProvider, metrics);
}
