using System.Security.Cryptography;

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
}
