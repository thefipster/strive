using Microsoft.AspNetCore.Components;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class AssimilateTab : ComponentBase
{
    private string? hash;

    private void OnHashSelected(string selectedHash)
    {
        hash = selectedHash;
        StateHasChanged();
    }
}
