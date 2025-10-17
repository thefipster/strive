using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Pages.Import.Tabs;

public partial class UploadTab : IAsyncDisposable
{
    private string _uploadKey = Guid.NewGuid().ToString();
    private HubConnection? _hubConnection;
    private string? _fileName;
    private int _progress;
    private bool _uploadDisabled;
    private IJSObjectReference? _module;
    private string _searchFilter = string.Empty;
    private MudTable<ZipIndex>? _fileTable;

    [Inject]
    public required IJSRuntime Js { get; set; }

    [Inject]
    public required UploadApi Uploader { get; set; }

    [Parameter]
    public string? Filter { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await ConnectHubs();
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _module = await Js.InvokeAsync<IJSObjectReference>("import", "/js/upload.js");
    }

    private async Task OnFilesChanged(IBrowserFile? arg)
    {
        if (_uploadDisabled || arg == null)
            return;

        _uploadDisabled = true;
        _fileName = arg.Name;

        if (_module == null)
            return;

        StateHasChanged();
        await _module.InvokeAsync<object>(
            "startUploadFromInput",
            Guid.NewGuid().ToString(),
            DotNetObjectReference.Create(this)
        );
    }

    [JSInvokable]
    public Task OnProgress(long uploadedBytes, long totalBytesReceived, string name)
    {
        _fileName = name;
        _progress = totalBytesReceived > 0 ? (int)(uploadedBytes * 100 / totalBytesReceived) : 0;
        InvokeAsync(StateHasChanged);
        return Task.CompletedTask;
    }

    [JSInvokable]
    public async Task OnCompleted(string finalName)
    {
        await ResetUpload();
    }

    [JSInvokable]
    public async Task OnError(string message)
    {
        await ResetUpload();
    }

    private void OnSearchFilterChanged(string search)
    {
        _searchFilter = search;
        _fileTable?.ReloadServerData();
    }

    private async Task<TableData<ZipIndex>> LoadServerData(TableState state, CancellationToken ct)
    {
        var page = await Uploader.GetZipPageAsync(
            new PagedRequest(state.Page, state.PageSize),
            _searchFilter
        );

        return new TableData<ZipIndex> { TotalItems = page.Total, Items = page.Items };
    }

    private async Task ResetUpload()
    {
        _uploadKey = Guid.NewGuid().ToString();
        await Task.Delay(200);
        _progress = 0;
        _fileName = null;
        _uploadDisabled = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task ConnectHubs()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7098" + Const.Hubs.Importer.Url)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string, bool>(
            Const.Hubs.Importer.ReportAction,
            (_, update) =>
            {
                if (update)
                {
                    _fileTable?.ReloadServerData();
                    InvokeAsync(StateHasChanged);
                }
            }
        );

        await _hubConnection.StartAsync();
        await JoinGroups();
        _hubConnection.Reconnected += async _ => await JoinGroups();
    }

    private async Task JoinGroups()
    {
        if (_hubConnection == null)
            return;

        await _hubConnection.InvokeAsync("JoinGroup", Const.Hubs.Importer.Actions.Unzip);
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            try
            {
                await _hubConnection.InvokeAsync("LeaveGroup", Const.Hubs.Importer.Actions.Unzip);
            }
            catch
            {
                // ignore if connection already lost
            }

            await _hubConnection.DisposeAsync();
        }
    }
}
