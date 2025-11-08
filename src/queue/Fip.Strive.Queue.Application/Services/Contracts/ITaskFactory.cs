using Fip.Strive.Queue.Application.Tasks;

namespace Fip.Strive.Queue.Application.Services.Contracts;

public interface ITaskFactory
{
    QueueRunner CreateRunner();
    QueueReporter CreateReporter();
}
