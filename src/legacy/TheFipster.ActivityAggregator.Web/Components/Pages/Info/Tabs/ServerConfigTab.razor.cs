using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Pages.Info.Tabs;

public partial class ServerConfigTab : ComponentBase
{
    [Inject]
    public required InfoApi Info { get; set; }

    private Dictionary<string, string> _values = [];

    protected override async Task OnParametersSetAsync()
    {
        _values = await Info.GetConfig();
    }
}
