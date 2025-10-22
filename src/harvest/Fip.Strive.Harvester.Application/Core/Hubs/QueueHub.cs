using Microsoft.AspNetCore.SignalR;

namespace Fip.Strive.Harvester.Application.Core.Hubs;

public class QueueHub : Hub
{
    public const string HubName = "Queue";

    public const string QueueReportMethodName = "QueueReport";
}
