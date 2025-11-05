using Fip.Strive.Core.Application.Infrastructure.Contracts;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Domain;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fip.Strive.Harvester.Web.Components.Pages;

[Route("/inventory")]
public partial class InventoryPage(IInventory inventory, ISpecificationReader<DataIndex> data)
    : ComponentBase
{
    private List<int> _years = [];

    private List<DateEntry> _entries = [];
    private int _selectedYear;
    private DateTime? _selectedDate;

    private MudTable<DataIndex>? _dataTable;

    [Parameter]
    [SupplyParameterFromQuery]
    public int? Year { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        _years = (await inventory.GetYearsAsync()).ToList();
        _selectedYear = Year ?? (_years.Count != 0 ? _years[^1] : DateTime.Now.Year);

        _entries = (await inventory.GetEntriesAsync(_selectedYear)).ToList();
    }

    private void OnDayClicked(int year, int month, int day)
    {
        _selectedDate = new DateTime(year, month, day);
        _dataTable?.ReloadServerData();
    }

    private Task<TableData<DataIndex>> OnDataIndexRequested(TableState state, CancellationToken ct)
    {
        if (_selectedDate == null)
            return Task.FromResult(new TableData<DataIndex> { Items = [], TotalItems = 0 });

        var specs = new PageSpecificationRequest<DataIndex>(
            x => x.Timestamp,
            false,
            state.Page,
            state.PageSize,
            x => x.Timestamp.Date == _selectedDate
        );

        var files = data.GetPaged(specs);

        return Task.FromResult(
            new TableData<DataIndex> { Items = files.Items, TotalItems = files.Total }
        );
    }
}
