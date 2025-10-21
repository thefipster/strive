using Microsoft.AspNetCore.Components;

namespace TheFipster.ActivityAggregator.Web.Components.Common;

public partial class NavMenu
{
    [Parameter]
    public EventCallback<string> DrawerToggled { get; set; }

    private void OnMenuToggleClick() => DrawerToggled.InvokeAsync();
}
