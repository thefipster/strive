using Microsoft.AspNetCore.Components;

namespace TheFipster.ActivityAggregator.Web.Components.Import.Scan;

public partial class FileList : ComponentBase
{
    [Parameter]
    public string UpdateId { get; set; }
}
