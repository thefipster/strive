using Microsoft.AspNetCore.Components;

namespace Fip.Strive.Harvester.Web.Components.Parts;

public partial class Headline : ComponentBase
{
    [Parameter]
    public required string Text { get; set; }
}
