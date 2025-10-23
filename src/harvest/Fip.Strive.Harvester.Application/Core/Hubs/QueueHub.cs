using Microsoft.AspNetCore.SignalR;

namespace Fip.Strive.Harvester.Application.Core.Hubs;

public class QueueHub : Hub
{
    public const string HubName = "queue";

    public const string QueueReportMethodName = "QueueReport";
}
