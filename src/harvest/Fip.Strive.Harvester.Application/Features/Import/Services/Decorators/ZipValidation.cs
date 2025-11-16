using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Application.Features.Import.Services.Decorators;

public class ZipValidation(IImportService component) : IImportService
{
    public Task<string?> MoveZipAsync(UploadSignal signal, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(signal.Filepath))
            throw new ArgumentException(
                "Zip path cannot be null or empty.",
                nameof(signal.Filepath)
            );

        if (!File.Exists(signal.Filepath))
            throw new FileNotFoundException(
                "The specified zip file does not exist.",
                signal.Filepath
            );

        return component.MoveZipAsync(signal, ct);
    }
}
