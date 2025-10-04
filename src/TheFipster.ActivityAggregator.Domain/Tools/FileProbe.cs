using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml;
using TheFipster.ActivityAggregator.Domain.Extensions;

namespace TheFipster.ActivityAggregator.Domain.Tools;

public class FileProbe
{
    private readonly Encoding _encoding = new UTF8Encoding(false);

    private readonly FileInfo _file;
    private readonly byte[] _buffer;

    public FileProbe(string filePath, int maxBytes = 4096, Encoding? customEncoding = null)
    {
        if (customEncoding != null)
            _encoding = customEncoding;

        _file = new FileInfo(filePath);
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var length = Math.Min(maxBytes, (int)fs.Length);
        _buffer = new byte[length];
        fs.ReadExactly(_buffer, 0, length);

        IsText = CheckIfTextFile();
        if (!IsText)
            return;

        Text = GetText();
        Lines = GetLines().ToList();
        JsonValues = TryGetJsonValues();
        if (JsonValues != null)
            JsonTags = JsonValues.Keys.ToHashSet();

        XmlTags = TryGetXmlTags().ToHashSet();
    }

    public string Filepath => _file.FullName;
    public ReadOnlyMemory<byte> Bytes => _buffer;
    public bool IsText { get; }
    public string? Text { get; }
    public List<string>? Lines { get; }
    public HashSet<string>? JsonTags { get; }
    public HashSet<string>? XmlTags { get; }
    public Dictionary<string, string?>? JsonValues { get; }

    private bool CheckIfTextFile()
    {
        if (_buffer.Length == 0)
            return false;

        int printable = 0;
        foreach (var b in _buffer)
        {
            if (b == 0)
                return false; // null byte → binary
            if ((b >= 0x20 && b <= 0x7E) || b == 0x09 || b == 0x0A || b == 0x0D)
                printable++;
        }
        double ratio = (double)printable / _buffer.Length;
        return ratio > 0.9;
    }

    private string GetText() => _encoding.GetString(_buffer);

    private IEnumerable<string> GetLines()
    {
        using var reader = new StringReader(GetText());
        string? line;
        while ((line = reader.ReadLine()) != null)
            yield return line;
    }

    private Dictionary<string, string?>? TryGetJsonValues()
    {
        try
        {
            return GetJsonValues();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private Dictionary<string, string?> GetJsonValues()
    {
        var props = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var reader = new Utf8JsonReader(_buffer, isFinalBlock: false, state: default);
        string? lastProp = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                lastProp = reader.GetString();
            }
            else if (lastProp != null)
            {
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
        return props;
    }

    private IEnumerable<string> TryGetXmlTags()
    {
        var elements = new HashSet<string>();
        var attributes = new HashSet<string>();

        try
        {
            GetXmlTags(elements, attributes);
        }
        catch
        {
            // Incomplete XML → just return what we collected so far
        }

        return elements.Concat(attributes);
    }

    private void GetXmlTags(HashSet<string> elements, HashSet<string> attributes)
    {
        var text = GetText();
        text = text.RemoveDoubleLineBreaks();
        text = text.ConsolidateLineBreaks();

        try
        {
            ScanTextForXmlTags(elements, attributes, text);
            return;
        }
        catch (Exception)
        {
            text = text.RemoveBom();
        }

        ScanTextForXmlTags(elements, attributes, text);
    }

    private static void ScanTextForXmlTags(
        HashSet<string> elements,
        HashSet<string> attributes,
        string text
    )
    {
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
}
