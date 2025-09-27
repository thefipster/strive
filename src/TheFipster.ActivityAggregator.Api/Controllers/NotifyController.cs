using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/notify")]
public class NotifyController : ControllerBase
{
    private readonly HubConnection connection;

    public NotifyController()
    {
        connection = new HubConnectionBuilder().WithUrl("https://localhost:7260/eventhub").Build();
        connection.StartAsync().Wait();
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEvent([FromBody] string message)
    {
        await connection.InvokeAsync("ReportProgress", message);
        return Ok();
    }
}
