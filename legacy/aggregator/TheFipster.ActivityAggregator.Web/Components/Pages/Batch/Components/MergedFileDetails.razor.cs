using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models.Files;

namespace TheFipster.ActivityAggregator.Web.Components.Pages.Batch.Components;

public partial class MergedFileDetails : ComponentBase
{
    [Parameter]
    public MergedFile? MergedFile { get; set; }
}
