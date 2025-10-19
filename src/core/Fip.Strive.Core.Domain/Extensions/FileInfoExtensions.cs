using HashifyNet;
using HashifyNet.Algorithms.XxHash3;

namespace Fip.Strive.Core.Domain.Extensions;

public static class FileInfoExtensions
{
    public static async Task<string> HashXx3Async(this FileInfo file, CancellationToken token)
    {
        var hasher = HashFactory<IXxHash3>.Create();
        await using var stream = File.OpenRead(file.FullName);
        var hash = await hasher.ComputeHashAsync(stream, token);
        return hash.AsHexString();
    }
}
