using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using HashifyNet;
using HashifyNet.Algorithms.XxHash3;

namespace Fip.Strive.Core.Application.Features.FileSystem.Services;

[ExcludeFromCodeCoverage]
public class FileHasher : IFileHasher
{
    public async Task<string> HashXx3Async(string filepath, CancellationToken ct)
    {
        var hasher = HashFactory<IXxHash3>.Create();
        await using var stream = File.OpenRead(filepath);

        var hash = await hasher.ComputeHashAsync(stream, ct);

        return hash.AsHexString();
    }
}
