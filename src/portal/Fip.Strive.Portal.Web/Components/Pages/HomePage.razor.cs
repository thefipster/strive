using Fip.Strive.Portal.Web.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Portal.Web.Components.Pages;

[Route("/")]
public partial class HomePage(IOptions<List<AppConfig>> config) : ComponentBase
{
    private List<AppConfig> _apps = [];

    protected override void OnParametersSet()
    {
        _apps = config.Value;
    }
}
