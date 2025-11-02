using Fip.Strive.Portal.Web.Models;
using Microsoft.AspNetCore.Components;

namespace Fip.Strive.Portal.Web.Components.Parts;

public partial class AppTile : ComponentBase
{
    [Parameter]
    public required AppConfig App { get; set; }
}
