using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Queue.Application.Tasks;

namespace Fip.Strive.Harvester.Application.Core.Queue;

public abstract class SignalQueueWorker(SignalTypes signal) : QueueWorker((int)signal);
