using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models.Files;

namespace TheFipster.ActivityAggregator.Web.Components.Batch;

public partial class MergedFileDetails : ComponentBase
{
    [Parameter]
    public MergedFile? MergedFile { get; set; }
}
