using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace TheFipster.ActivityAggregator.Web.Components.Import.Upload;

public partial class MultiUpload : ComponentBase
{
    private readonly Dictionary<string, UploadState> uploads = new();

    private IJSObjectReference? module;

    [Inject]
    public IJSRuntime? Js { get; set; }

    [Parameter]
    public string Title { get; set; } = "File Upload";

    [Parameter]
    public string? Filter { get; set; }

    [Parameter]
    public string Icon { get; set; } = Icons.Material.Filled.UploadFile;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Js != null)
            module = await Js.InvokeAsync<IJSObjectReference>("import", "/js/upload.js");
    }

    private async Task OnFilesChanged(IReadOnlyList<IBrowserFile>? files)
    {
        if (files is null || files.Count == 0)
            return;

        foreach (var file in files)
        {
            var uploadId = Guid.NewGuid().ToString();
            var state = new UploadState
            {
                FileName = file.Name,
                TotalBytes = file.Size,
                UploadedBytes = 0,
                Progress = 0,
            };
            uploads[uploadId] = state;
            StateHasChanged();

            if (module != null)
            {
                // Pass the file directly to JS instead of querying the DOM
                await module.InvokeVoidAsync(
                    "startUpload",
                    file,
                    DotNetObjectReference.Create(this),
                    uploadId
                );
            }
        }
    }

    [JSInvokable]
    public Task OnProgress(string uploadId, long uploadedBytes, long totalBytes)
    {
        if (uploads.TryGetValue(uploadId, out var state))
        {
            state.UploadedBytes = uploadedBytes;
            state.TotalBytes = totalBytes;
            state.Progress = (int)((uploadedBytes * 100L) / totalBytes);
            InvokeAsync(StateHasChanged);
        }
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnCompleted(string uploadId)
    {
        if (uploads.TryGetValue(uploadId, out var state))
        {
            state.Progress = 100;
            InvokeAsync(StateHasChanged);
        }
        return Task.CompletedTask;
    }

    class UploadState
    {
        public string FileName = "";
        public long TotalBytes;
        public long UploadedBytes;
        public int Progress;
    }
}
