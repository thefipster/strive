using System.Diagnostics;
using Fip.Strive.Harvester.Application.Features.Upload.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Upload.Services;

public class UploadService(IOptions<UploadConfig> config) : IUploadService
{
    private readonly int _uploadBufferSize = 80 * 1024;
    private readonly Stopwatch _stopwatch = new();
    private TimeSpan _reportInterval = TimeSpan.FromMilliseconds(100);

    public event EventHandler<int>? ProgressChanged;

    public void SetReportInterval(TimeSpan interval) => _reportInterval = interval;

    public async Task<string> SaveUploadAsync(string filename, Stream stream, CancellationToken ct)
    {
        _stopwatch.Start();

        var filePath = Path.Combine(config.Value.Path, filename);
        await using var fs = new FileStream(filePath, FileMode.Create);

        var buffer = new byte[_uploadBufferSize];
        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
        {
            await fs.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalBytesRead += bytesRead;
            Report(stream, totalBytesRead);
            ct.ThrowIfCancellationRequested();
        }

        ProgressChanged?.Invoke(this, 100);
        _stopwatch.Reset();

        return filePath;
    }

    private void Report(Stream stream, long totalBytesRead)
    {
        if (_stopwatch.Elapsed < _reportInterval)
            return;

        var uploadProgress = (int)(100 * totalBytesRead / stream.Length);
        ProgressChanged?.Invoke(this, uploadProgress);
        _stopwatch.Restart();
    }
}
