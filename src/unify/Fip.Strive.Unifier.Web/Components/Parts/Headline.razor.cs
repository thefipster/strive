using Microsoft.AspNetCore.Components;

namespace Fip.Strive.Unifier.Web.Components.Parts;

public partial class Headline : ComponentBase
{
    [Parameter]
    public required string Text { get; set; }
}
