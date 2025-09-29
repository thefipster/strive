using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheFipster.ActivityAggregator.Web.Components.Import.Scan;

public partial class ScanActions : ComponentBase
{
    [Parameter]
    public EventCallback ScanClicked { get; set; }

    [Parameter]
    public bool ActionDisabled { get; set; }

    [Parameter]
    public string Title { get; set; } = "Scan uploaded files";

    [Parameter]
    public string Icon { get; set; } = Icons.Material.Filled.Radar;

    private async Task OnScanClicked() => await ScanClicked.InvokeAsync();
}
