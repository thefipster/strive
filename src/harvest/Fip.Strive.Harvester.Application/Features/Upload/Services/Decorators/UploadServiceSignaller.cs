using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Upload.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Upload.Signals;

namespace Fip.Strive.Harvester.Application.Features.Upload.Services.Decorators;

public class UploadServiceSignaller(IUploadService component, ISignalQueue queue) : IUploadService
{
    public event EventHandler<int>? ProgressChanged
    {
        add => component.ProgressChanged += value;
        remove => component.ProgressChanged -= value;
    }

    public void SetReportInterval(TimeSpan interval) => component.SetReportInterval(interval);

    public async Task<string> SaveUploadAsync(string filename, Stream stream, CancellationToken ct)
    {
        var filepath = await component.SaveUploadAsync(filename, stream, ct);
        var signal = UploadSignal.From(filepath);

        await queue.EnqueueAsync(signal, ct);

        return filepath;
    }
}
