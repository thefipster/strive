namespace Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;

public interface IFileHasher
{
    Task<string> HashXx3Async(string filepath, CancellationToken ct);
}
