using Microsoft.AspNetCore.Components;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class ScanTab : ComponentBase
{
    private string? hash;

    private void OnHashSelected(string selectedHash)
    {
        hash = selectedHash;
        StateHasChanged();
    }
}
