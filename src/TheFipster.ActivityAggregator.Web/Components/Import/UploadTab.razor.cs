using Microsoft.AspNetCore.Components;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class UploadTab : ComponentBase
{
    private int blah;
    
    protected override Task OnParametersSetAsync()
    {
        blah = new Random().Next(1, 100);
        
        return base.OnParametersSetAsync();
    }
}