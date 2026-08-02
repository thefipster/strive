using TheFipster.ActivityAggregator.Api.Features.Info.Mediators.Contracts;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Services;

namespace TheFipster.ActivityAggregator.Api.Features.Core.Controllers;

[ApiController]
[Route("api/info")]
public class InfoController(IConfigAction configAction) : ControllerBase
{
    [HttpGet("config")]
    public Dictionary<string, string> GetYearlyInventory() => configAction.GetConfiguration();
}
