using System.Text.RegularExpressions;

namespace TheFipster.ActivityAggregator.Domain.Tools;

public static class FileHelper
{
    public static string GetUniqueFilePath(string path)
    {
        if (!File.Exists(path))
            return path;

        var dir = Path.GetDirectoryName(path) ?? "";
        var name = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);
        int i = 1;
        string candidate;

        do
        {
            candidate = Path.Combine(dir, $"{name}({i++}){ext}");
        } while (File.Exists(candidate));

        return candidate;
    }

    public static string SanitizeFileName(string input)
    {
        var file = Path.GetFileName(input);
        file = Regex.Replace(file, @"[^A-Za-z0-9\._\-() ]+", "");
        return file;
    }
}
