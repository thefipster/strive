using Fip.Strive.Harvester.Application.Core.Queue;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts.Base;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Infrastructure.Contexts;

public class SignalQueueContext(IOptions<QueueConfig> config)
    : SimpleContext(config.Value.DatabasePath);
