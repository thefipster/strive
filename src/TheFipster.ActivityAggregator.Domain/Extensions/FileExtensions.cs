using System.Security.Cryptography;
using HashifyNet;
using HashifyNet.Algorithms.XxHash3;

namespace TheFipster.ActivityAggregator.Domain.Extensions;

public static class FileExtensions
{
    public static string GetMd5Hash(this FileInfo file)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(file.FullName);

        var hash = md5.ComputeHash(stream);
        var hashString = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

        return hashString;
    }

    public static async Task<string> HashXx3Async(this FileInfo file, CancellationToken token)
    {
        var hasher = HashFactory<IXxHash3>.Create();
        await using var stream = File.OpenRead(file.FullName);
        var hash = await hasher.ComputeHashAsync(stream, token);
        return hash.AsHexString();
    }
}
