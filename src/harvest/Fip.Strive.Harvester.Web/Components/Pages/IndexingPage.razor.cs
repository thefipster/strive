using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fip.Strive.Harvester.Web.Components.Pages;

[Route("/indexing")]
public partial class IndexingPage(IZipRepository zips, IFileRepository files, IDataRepository data)
    : ComponentBase
{
    private ZipIndex? _selectedZip;
    private FileInstance? _selectedFile;

    private MudTable<FileInstance>? _fileTable;
    private MudTable<DataIndex>? _dataTable;

    private async Task<TableData<ZipIndex>> OnZipIndexRequested(
        TableState state,
        CancellationToken ct
    )
    {
        var page = await zips.GetPageAsync(state.Page, state.PageSize, ct);
        return new TableData<ZipIndex> { Items = page.Items, TotalItems = page.Total };
    }

    private void OnZipIndexRowClick(TableRowClickEventArgs<ZipIndex> obj)
    {
        _selectedFile = null;
        _dataTable?.ReloadServerData();

        _selectedZip = obj.Item;
        _fileTable?.ReloadServerData();
    }

    private async Task<TableData<FileInstance>> OnFileIndexRequested(
        TableState state,
        CancellationToken ct
    )
    {
        if (_selectedZip == null)
            return new TableData<FileInstance> { Items = [], TotalItems = 0 };

        var page = await files.GetPageAsync(_selectedZip.Filepath, state.Page, state.PageSize, ct);

        return new TableData<FileInstance> { Items = page.Items, TotalItems = page.Total };
    }

    private void OnFileIndexRowClick(TableRowClickEventArgs<FileInstance> obj)
    {
        _selectedFile = obj.Item;
        _dataTable?.ReloadServerData();
    }

    private async Task<TableData<DataIndex>> OnDataIndexRequested(
        TableState state,
        CancellationToken ct
    )
    {
        if (_selectedFile == null)
            return new TableData<DataIndex> { Items = [], TotalItems = 0 };

        var page = await data.GetPageAsync(_selectedFile.Hash, state.Page, state.PageSize, ct);

        return new TableData<DataIndex> { Items = page.Items, TotalItems = page.Total };
    }
}
