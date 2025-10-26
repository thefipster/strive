using System.Text;

namespace Fip.Strive.Core.Domain.Extensions;

public static class TextExtensions
{
    public static string RemoveBom(this string text)
    {
        var byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
        if (text.StartsWith(byteOrderMarkUtf8))
            return text.Remove(0, byteOrderMarkUtf8.Length);

        return text;
    }

    public static string RemoveDoubleLineBreaks(this string text)
    {
        while (text.Contains("\n\n"))
        {
            text = text.Replace("\n\n", "\n");
        }

        return text;
    }

    public static string ConsolidateLineBreaks(this string text) => text.Replace("\r\n", "\n");
}
