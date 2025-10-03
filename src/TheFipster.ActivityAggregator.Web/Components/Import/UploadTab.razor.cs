using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class UploadTab : ComponentBase
{
    private string uploadKey = Guid.NewGuid().ToString();
    private HubConnection? hubConnection;
    private IEnumerable<ZipIndex> zips = [];
    private bool isLoading = true;
    private string? fileName;
    private int progress;
    private bool uploadDisabled;
    private IJSObjectReference? module;

    [Inject]
    public IJSRuntime? Js { get; set; }

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public UploadApi? UploadService { get; set; }

    [Parameter]
    public string? Filter { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (UploadService != null)
            zips = (await UploadService.GetZipsAsync()).OrderByDescending(x => x.IndexedAt);

        await ConnectHubs();
        await base.OnParametersSetAsync();
        isLoading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Js != null)
            module = await Js.InvokeAsync<IJSObjectReference>("import", "/js/upload.js");
    }

    private async Task OnFilesChanged(IBrowserFile? arg)
    {
        if (uploadDisabled || arg == null)
            return;

        uploadDisabled = true;
        fileName = arg.Name;

        if (module == null)
            return;

        StateHasChanged();
        await module.InvokeAsync<object>(
            "startUploadFromInput",
            Guid.NewGuid().ToString(),
            DotNetObjectReference.Create(this)
        );
    }

    [JSInvokable]
    public Task OnProgress(long uploadedBytes, long totalBytesReceived, string name)
    {
        fileName = name;
        progress = totalBytesReceived > 0 ? (int)(uploadedBytes * 100 / totalBytesReceived) : 0;
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

    private async Task ResetUpload()
    {
        uploadKey = Guid.NewGuid().ToString();
        await Task.Delay(300);
        progress = 0;
        fileName = null;
        uploadDisabled = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task ConnectHubs()
    {
        if (Navigation is null)
            return;

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri(Const.Hubs.Ingester.Url))
            .Build();

        hubConnection.On<string, string>(
            Const.Hubs.Ingester.UnzipFinished,
            (_, _) =>
            {
                InvokeAsync(async () =>
                {
                    if (UploadService != null)
                        zips = await UploadService.GetZipsAsync();

                    StateHasChanged();
                });
            }
        );

        await hubConnection.StartAsync();
    }
}
