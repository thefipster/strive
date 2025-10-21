using Fip.Strive.Harvester.Application.Features.Upload.Services.Contracts;
using Microsoft.AspNetCore.Components.Forms;

namespace Fip.Strive.Harvester.Web.Components.Layout.Components.NavMenuItems;

public partial class UploadButton(IUploadService uploadService)
{
    private readonly long _sizeLimit = 5L * 1024 * 1024 * 1024;
    private int _uploadProgress;
    private bool _isUploading;

    protected override void OnInitialized()
    {
        uploadService.ProgressChanged += OnProgressChanged;
    }

    private async Task OnFileUpload(IBrowserFile? file)
    {
        if (file == null)
            return;

        _isUploading = true;
        await using var stream = file.OpenReadStream(maxAllowedSize: _sizeLimit);
        await uploadService.SaveUploadAsync(file.Name, stream, CancellationToken.None);
    }

    private async void OnProgressChanged(object? sender, int progressPercent)
    {
        _uploadProgress = progressPercent;
        await InvokeAsync(StateHasChanged);

        if (_uploadProgress >= 100)
        {
            await Task.Delay(300);
            _isUploading = false;
            _uploadProgress = 0;
            await InvokeAsync(StateHasChanged);
        }
    }
}
