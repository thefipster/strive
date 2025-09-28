using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace TheFipster.ActivityAggregator.Web.Components.Import.Upload;

public partial class ChunkUpload : ComponentBase
{
    private string? fileName;
    private string? uploadId;
    private int progress;
    private bool uploadDisabled;
    private IJSObjectReference? module;

    [Parameter]
    public EventCallback UploadFinished { get; set; }

    [Parameter]
    public string Title { get; set; } = "File Upload";

    [Parameter]
    public string? Filter { get; set; }

    [Parameter]
    public string Icon { get; set; } = Icons.Material.Filled.UploadFile;

    [Inject]
    public IJSRuntime? Js { get; set; }

    [Inject]
    public NavigationManager? Navigation { get; set; }

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
        uploadId = Guid.NewGuid().ToString();

        if (module == null)
            return;

        StateHasChanged();
        await module.InvokeAsync<object>(
            "startUploadFromInput",
            uploadId,
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
        await Task.Delay(100);
        await UploadFinished.InvokeAsync(null);
        // if (Navigation != null)
        //     Navigation.NavigateTo(Navigation.Uri, true);

        //progress = 100;
        // fileName = null;
        // progress = 0;
        // uploadDisabled = false;
        // uploadId = null;
        // await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public Task OnError(string message)
    {
        InvokeAsync(StateHasChanged);
        return Task.CompletedTask;
    }
}
