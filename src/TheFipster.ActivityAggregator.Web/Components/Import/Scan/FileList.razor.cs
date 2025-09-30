using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheFipster.ActivityAggregator.Web.Components.Import.Scan;

public partial class FileList : ComponentBase
{
    [Parameter]
    public string UpdateId { get; set; }
}
