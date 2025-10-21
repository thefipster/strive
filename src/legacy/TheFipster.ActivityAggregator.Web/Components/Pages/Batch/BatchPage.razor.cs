using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Pages.Batch;

public partial class BatchPage : ComponentBase
{
    private DateTime? _date;
    private IEnumerable<BatchIndex>? _batches;

    [Inject]
    public required BatchApi Batcher { get; set; }

    [Parameter]
    [SupplyParameterFromQuery]
    public string? Date { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrWhiteSpace(Date))
        {
            if (DateTime.TryParse(Date, out DateTime date))
            {
                _date = date;
            }
        }

        if (_date.HasValue)
            _batches = await Batcher.GetDayAsync(_date.Value);
    }
}
