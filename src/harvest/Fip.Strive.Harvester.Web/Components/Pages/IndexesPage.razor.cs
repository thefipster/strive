using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Repositories.Contracts;
using MudBlazor;

namespace Fip.Strive.Harvester.Web.Components.Pages;

public partial class IndexesPage(
    ISpecificationReader<ZipIndex> zipPager,
    ISpecificationReader<FileIndex> filePager
)
{
    private ZipIndex? _selectedZip;
    private MudTable<FileIndex>? _fileTable;

    private Task<TableData<ZipIndex>> OnZipIndexRequested(TableState state, CancellationToken ct)
    {
        var specs = new PageSpecificationRequest<ZipIndex>(
            x => x.SignalledAt,
            false,
            state.Page,
            state.PageSize
        );

        var zips = zipPager.GetPaged(specs);

        return Task.FromResult(
            new TableData<ZipIndex> { Items = zips.Items, TotalItems = zips.Total }
        );
    }

    private Task<TableData<FileIndex>> OnFileIndexRequested(TableState state, CancellationToken ct)
    {
        if (_selectedZip == null)
            return Task.FromResult(new TableData<FileIndex> { Items = [], TotalItems = 0 });

        var specs = new PageSpecificationRequest<FileIndex>(
            x => x.SignalledAt,
            false,
            state.Page,
            state.PageSize,
            x => x.ReferenceId == _selectedZip.ReferenceId
        );

        var files = filePager.GetPaged(specs);

        return Task.FromResult(
            new TableData<FileIndex> { Items = files.Items, TotalItems = files.Total }
        );
    }

    private void OnZipIndexRowClick(TableRowClickEventArgs<ZipIndex> obj)
    {
        _selectedZip = obj.Item;
        if (_fileTable != null)
            _fileTable.ReloadServerData();
    }
}
