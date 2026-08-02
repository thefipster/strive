using Fip.Strive.Application.Features.Catalog.Models;
using Fip.Strive.Application.Features.Catalog.Services.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fip.Strive.Web.Components.Pages;

public partial class CatalogPage
{
    [Inject]
    private ICatalogReader Catalog { get; set; } = default!;

    private MudTable<CatalogEntryRow>? _table;
    private IReadOnlyList<OccurrenceRow>? _occurrences;
    private CatalogEntryRow? _selected;
    private string? _search;
    private bool _detailOpen;

    private async Task<TableData<CatalogEntryRow>> LoadAsync(
        TableState state,
        CancellationToken cancellationToken
    )
    {
        var page = await Catalog.GetEntriesAsync(
            state.Page * state.PageSize,
            state.PageSize,
            _search,
            cancellationToken
        );

        return new TableData<CatalogEntryRow> { Items = page.Items, TotalItems = page.TotalCount };
    }

    private async Task OnSearchChangedAsync(string? search)
    {
        _search = search;

        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task ShowOccurrencesAsync(CatalogEntryRow entry)
    {
        _selected = entry;
        _occurrences = null;
        _detailOpen = true;

        _occurrences = await Catalog.GetOccurrencesAsync(entry.Hash);
    }
}
