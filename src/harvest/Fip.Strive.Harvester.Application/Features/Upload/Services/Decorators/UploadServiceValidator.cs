using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Harvester.Application.Features.Upload.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Upload.Services.Decorators;

public class UploadServiceValidator(IUploadService component, IOptions<UploadConfig> config)
    : IUploadService
{
    public event EventHandler<int>? ProgressChanged
    {
        add => component.ProgressChanged += value;
        remove => component.ProgressChanged -= value;
    }

    public void SetReportInterval(TimeSpan interval)
    {
        if (interval < TimeSpan.FromMilliseconds(100))
            interval = TimeSpan.FromMilliseconds(100);

        component.SetReportInterval(interval);
    }

    public async Task<string> SaveUploadAsync(string filename, Stream stream, CancellationToken ct)
    {
        Validate(filename);
        return await component.SaveUploadAsync(filename, stream, ct);
    }

    private void Validate(string filename)
    {
        var uploadDir = config.Value.Path;

        if (string.IsNullOrEmpty(uploadDir))
            throw new ConfigurationException(nameof(config.Value.Path), "Is null or empty.");

        try
        {
            Directory.CreateDirectory(uploadDir);
        }
        catch (Exception e)
        {
            throw new UploadException(filename, $"Unable to create directory {uploadDir}", e);
        }

        var filePath = Path.Combine(uploadDir, filename);
        if (File.Exists(filePath))
            throw new UploadException(filename, $"File {filename} already exists.");
    }
}
