using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace TheFipster.ActivityAggregator.Domain.Tools;

public class FileProbe
{
    private readonly byte[] buffer;
    private readonly Encoding encoding;
    private readonly FileInfo file;

    public FileProbe(string filePath, int maxBytes = 4096, Encoding? encoding = null)
    {
        file = new FileInfo(filePath);
        this.encoding = encoding ?? Encoding.UTF8;

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var length = Math.Min(maxBytes, (int)fs.Length);
        buffer = new byte[length];
        fs.ReadExactly(buffer, 0, length);
    }

    public string Filepath => file.FullName;

    public ReadOnlyMemory<byte> GetBytes => buffer;

    public string GetText() => encoding.GetString(buffer);

    public IEnumerable<string> GetLines()
    {
        using var reader = new StringReader(GetText());
        string? line;
        while ((line = reader.ReadLine()) != null)
            yield return line;
    }

    public bool IsText()
    {
        if (buffer.Length == 0)
            return false;

        int printable = 0;
        foreach (var b in buffer)
        {
            if (b == 0)
                return false; // null byte → binary
            if ((b >= 0x20 && b <= 0x7E) || b == 0x09 || b == 0x0A || b == 0x0D)
                printable++;
        }
        double ratio = (double)printable / buffer.Length;
        return ratio > 0.9;
    }

    /// Collect property names only
    public HashSet<string> GetJsonPropertyNames()
    {
        var props = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!IsText())
            return props;

        var reader = new Utf8JsonReader(buffer, isFinalBlock: false, state: default);

        try
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    props.Add(reader.GetString() ?? "");
                }
            }
        }
        catch (Exception)
        {
            return [];
        }

        return props;
    }

    /// Collect a dictionary of property -> first encountered value (as string)
    public Dictionary<string, string?> GetJsonPropertiesWithValues()
    {
        var props = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (!IsText())
            return props;

        var reader = new Utf8JsonReader(buffer, isFinalBlock: false, state: default);
        string? lastProp = null;

        try
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    lastProp = reader.GetString();
                }
                else if (lastProp != null)
                {
                    // Record first value we see for this property
                    if (!props.ContainsKey(lastProp))
                    {
                        string? value = reader.TokenType switch
                        {
                            JsonTokenType.String => reader.GetString(),
                            JsonTokenType.Number => reader.TryGetInt64(out var i)
                                ? i.ToString()
                                : reader.GetDouble().ToString(CultureInfo.InvariantCulture),
                            JsonTokenType.True => "true",
                            JsonTokenType.False => "false",
                            JsonTokenType.Null => "null",
                            _ => null, // skip objects/arrays for simplicity
                        };

                        props[lastProp] = value;
                    }
                    lastProp = null;
                }
            }
        }
        catch (Exception)
        {
            return new();
        }

        return props;
    }

    public IEnumerable<string> GetXmlPropsAndAttributes()
    {
        var elements = new HashSet<string>();
        var attributes = new HashSet<string>();

        try
        {
            var text = GetText();
            using var reader = XmlReader.Create(
                new StringReader(text),
                new XmlReaderSettings
                {
                    IgnoreComments = true,
                    IgnoreWhitespace = true,
                    DtdProcessing = DtdProcessing.Ignore,
                }
            );

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    elements.Add(reader.LocalName);

                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            attributes.Add(reader.LocalName);
                        }
                        reader.MoveToElement();
                    }
                }
            }
        }
        catch
        {
            // Incomplete XML → just return what we collected so far
        }

        return elements.Concat(attributes);
    }
}
