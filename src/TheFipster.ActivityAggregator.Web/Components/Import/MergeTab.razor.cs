using Microsoft.AspNetCore.Components;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class MergeTab : ComponentBase
{
    private int? year;

    private void OnYearSelected(int selectedYear)
    {
        year = selectedYear;
    }
}
