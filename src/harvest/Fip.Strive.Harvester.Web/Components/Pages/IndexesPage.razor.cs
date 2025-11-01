using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Repositories.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fip.Strive.Harvester.Web.Components.Pages;

[Route("/indexes")]
public partial class IndexesPage(
    ISpecificationReader<ZipIndex> zipPager,
    ISpecificationReader<FileIndex> filePager,
    ISpecificationReader<DataIndex> dataPager
) : ComponentBase
{
    private MudTable<FileIndex>? _fileTable;
    private MudTable<DataIndex>? _dataTable;
    private ZipIndex? _selectedZip;
    private FileIndex? _selectedFile;

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

    private void OnZipIndexRowClick(TableRowClickEventArgs<ZipIndex> obj)
    {
        _selectedZip = obj.Item;

        if (_selectedZip != null)
            _fileTable?.ReloadServerData();
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
            x => x.ParentId == _selectedZip.Hash
        );

        var files = filePager.GetPaged(specs);

        return Task.FromResult(
            new TableData<FileIndex> { Items = files.Items, TotalItems = files.Total }
        );
    }

    private void OnFileIndexRowClick(TableRowClickEventArgs<FileIndex> obj)
    {
        _selectedFile = obj.Item;

        if (_selectedFile != null)
            _dataTable?.ReloadServerData();
    }

    private Task<TableData<DataIndex>> OnDataIndexRequested(TableState state, CancellationToken ct)
    {
        if (_selectedFile == null)
            return Task.FromResult(new TableData<DataIndex> { Items = [], TotalItems = 0 });

        var specs = new PageSpecificationRequest<DataIndex>(
            x => x.SignalledAt,
            false,
            state.Page,
            state.PageSize,
            x => x.ParentId == _selectedFile.Hash
        );

        var files = dataPager.GetPaged(specs);

        return Task.FromResult(
            new TableData<DataIndex> { Items = files.Items, TotalItems = files.Total }
        );
    }
}
