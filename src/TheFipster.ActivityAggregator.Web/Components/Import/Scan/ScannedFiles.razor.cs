using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import.Scan;

public partial class ScannedFiles : ComponentBase
{
    private bool isLoading = true;
    private IEnumerable<ScannerIndex> indexes = [];

    [Inject]
    public ApiService? Api { get; set; }

    [Parameter]
    public string? Hash { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        isLoading = true;

        if (Api != null && Hash != null)
        {
            indexes = (await Api.GetScannerIndexesAsync(Hash)).OrderByDescending(x => x.IndexedAt);
            isLoading = false;
        }

        await base.OnParametersSetAsync();
    }
}
