using Fip.Strive.Core.Application.Infrastructure.Contexts;
using Fip.Strive.Queue.Domain.Models;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Queue.Application.Contexts;

public class SignalQueueContext(IOptions<QueueConfig> config)
    : LiteBaseContext(config.Value.DatabasePath);
