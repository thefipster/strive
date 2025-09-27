using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class ImportExtractor : ComponentBase
{
    private IEnumerable<ImporterIndex> indexes = [];
    private bool showPopover;
    private bool indexLoading = true;

    [Inject]
    public ApiService? Api { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await UpdateUploadsAsync();
        await base.OnParametersSetAsync();
    }

    private async Task OnSyncClickedAsync()
    {
        await UpdateUploadsAsync();
    }

    private async Task UpdateUploadsAsync()
    {
        if (Api != null)
        {
            indexes = (await Api.GetImporterIndexesAsync()).OrderByDescending(x => x.IndexedAt);
            indexLoading = false;
        }

        await InvokeAsync(StateHasChanged);
    }

    private void OnToggleOpen()
    {
        showPopover = !showPopover;
    }
}
