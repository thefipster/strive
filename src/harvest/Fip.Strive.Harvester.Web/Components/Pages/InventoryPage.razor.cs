using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Microsoft.AspNetCore.Components;

namespace Fip.Strive.Harvester.Web.Components.Pages;

[Route("/inventory")]
public partial class InventoryPage(IInventory inventory, NavigationManager navigation)
    : ComponentBase
{
    private List<int> _years = [];

    private List<DateEntry> _entries = [];
    private int _selectedYear;

    [Parameter]
    [SupplyParameterFromQuery]
    public int? Year { get; set; }

    protected override void OnParametersSet()
    {
        _years = inventory.GetYears().ToList();
        _selectedYear = Year ?? _years[^1];
        _entries = inventory.GetEntries(_selectedYear).ToList();
    }
}
