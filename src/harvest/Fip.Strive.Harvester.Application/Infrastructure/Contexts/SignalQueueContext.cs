using Fip.Strive.Core.Application.Infrastructure.Contexts;
using Fip.Strive.Harvester.Application.Core.Queue;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Infrastructure.Contexts;

public class SignalQueueContext(IOptions<QueueConfig> config)
    : LiteBaseContext(config.Value.DatabasePath);
