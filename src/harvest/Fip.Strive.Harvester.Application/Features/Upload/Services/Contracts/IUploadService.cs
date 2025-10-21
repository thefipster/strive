namespace Fip.Strive.Harvester.Application.Features.Upload.Services.Contracts;

public interface IUploadService
{
    event EventHandler<int>? ProgressChanged;

    void SetReportInterval(TimeSpan interval);

    Task<string> SaveUploadAsync(string filename, Stream stream, CancellationToken ct);
}
