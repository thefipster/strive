using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheFipster.ActivityAggregator.Web.Components;

public partial class FipFileSize : ComponentBase
{
    private const double Gb = 1024 * 1024 * 1024;
    private const double Mb = 1024 * 1024;
    private const double Kb = 1024;

    [Parameter]
    public long Size { get; set; }

    [Parameter]
    [Category(CategoryTypes.Text.Appearance)]
    public Typo Typo { get; set; } = Typo.body1;

    private string _sizeText = string.Empty;

    protected override void OnParametersSet()
    {
        double size;
        var unit = "b";

        if (Size > Gb)
        {
            size = Size / Gb;
            unit = "gb";
        }
        else if (Size > Mb)
        {
            size = Size / Mb;
            unit = "mb";
        }
        else if (Size > Kb)
        {
            size = Size / Kb;
            unit = "kb";
        }
        else
        {
            size = Size;
        }

        _sizeText = $"{size:0.#} {unit}";

        StateHasChanged();

        base.OnParametersSet();
    }
}
