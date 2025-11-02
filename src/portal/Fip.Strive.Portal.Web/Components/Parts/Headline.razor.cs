using Microsoft.AspNetCore.Components;

namespace Fip.Strive.Portal.Web.Components.Parts;

public partial class Headline : ComponentBase
{
    [Parameter]
    public required string Text { get; set; }
}
