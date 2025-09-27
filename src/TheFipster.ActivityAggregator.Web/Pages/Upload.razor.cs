using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace TheFipster.ActivityAggregator.Web.Pages;

public partial class Upload
{
    private string? fileName;
    private int progress; // percent 0..100
    private bool isUploading = false;
    private bool uploadDisabled = false;

    [Inject]
    public IJSRuntime JS { get; set; }

    DotNetObjectReference<Upload>? dotNetRef;
    IJSObjectReference? module;
    string uploadId = Guid.NewGuid().ToString();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        module = await JS.InvokeAsync<IJSObjectReference>("import", "/js/upload.js");
    }

    private async Task OnFilesChanged(IBrowserFile arg)
    {
        if (uploadDisabled)
            return;

        uploadDisabled = true;
        // We need to pass the file entry to JS; Input files must be retrieved via JS for slicing.
        // We'll call a JS function that shows the native file picker and returns file metadata.
        // Simpler: call a JS helper to grab the first file from the <input type=file>.
        if (module == null)
            return;

        var result = await module.InvokeAsync<object>(
            "startUploadFromInput",
            uploadId,
            DotNetObjectReference.Create(this)
        );
        // startUploadFromInput will call back to DotNet method OnProgress and eventually OnCompleted or OnError.
        isUploading = true;
    }

    [JSInvokable]
    public Task OnProgress(long uploadedBytes, long totalBytesReceived, string name)
    {
        // uploadedBytes: total bytes uploaded so far
        // totalBytesReceived: known total size
        fileName = name;
        progress = totalBytesReceived > 0 ? (int)((uploadedBytes * 100) / totalBytesReceived) : 0;
        InvokeAsync(StateHasChanged);
        return Task.CompletedTask;
    }

    [JSInvokable]
    public async Task OnCompleted(string finalName)
    {
        isUploading = false;
        progress = 100;
        await InvokeAsync(StateHasChanged);
        await Task.Delay(2000);
        progress = 0;
        fileName = null;
        uploadDisabled = false;
        StateHasChanged();
    }

    [JSInvokable]
    public Task OnError(string message)
    {
        isUploading = false;
        InvokeAsync(StateHasChanged);
        return Task.CompletedTask;
    }
}
