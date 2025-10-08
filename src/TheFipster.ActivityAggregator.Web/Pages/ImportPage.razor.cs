using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheFipster.ActivityAggregator.Web.Pages;

public partial class ImportPage : ComponentBase
{
    private MudTabs? _tabs;
    private bool _isRendered;

    [Parameter]
    [SupplyParameterFromQuery]
    public string? Tab { get; set; }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && _tabs != null && !string.IsNullOrWhiteSpace(Tab))
            _tabs.ActivatePanel(Tab);

        base.OnAfterRender(firstRender);
        _isRendered = true;
    }
}
