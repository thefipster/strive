using Microsoft.AspNetCore.Components;
using MudBlazor;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Pages;

public partial class BatchPage : ComponentBase
{
    private int _year = DateTime.Now.Year;
    private DateTime? _date;
    private IEnumerable<BatchIndex>? _batches;
    private MudTabs? _tabs;

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public BatchApi? Batcher { get; set; }

    [Inject]
    public HistoryApi? History { get; set; }

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
                _year = date.Year;
            }
        }

        if (Batcher != null && _date.HasValue)
            _batches = await Batcher.GetDayAsync(_date.Value);

        await base.OnParametersSetAsync();
    }

    private void OnBackClicked()
    {
        if (Navigation == null)
            return;

        Navigation.NavigateTo("import?tab=batch&year=" + _year);
    }
}
