using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Controllers;

[ApiController]
[Route("api/assimilate")]
public class AssimilateController(
    IExtractPageAction pageAction,
    IExtractorsAction extractorsAction,
    IAssimilateAction assimilateAction
) : ControllerBase
{
    [HttpGet("extracts")]
    public PagedResponse<ExtractorIndex> GetExtractPage(
        [FromQuery] AssimilateExtractPageRequest request
    ) => pageAction.GetExtractPage(request);

    [HttpGet("extractors")]
    public Dictionary<DataSources, int> GetExtractors() => extractorsAction.GetExtractors();

    [HttpGet]
    public void Assimilate() => assimilateAction.Assimilate();
}
