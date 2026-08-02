using Microsoft.AspNetCore.Components;
using MudBlazor;
using TheFipster.ActivityAggregator.Web.Models;

namespace TheFipster.ActivityAggregator.Web.Components.Pages.Batch.Components;

public partial class ImportHistory : ComponentBase
{
    [Parameter]
    public List<CommonIndex> Indexes { get; set; } = [];

    private readonly TableGroupDefinition<CommonIndex> _groupDefinition = new()
    {
        GroupName = "Stage",
        Indentation = true,
        Expandable = false,
        Selector = e => e.Type.ToUpper(),
    };
}
