using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Components;

public abstract class JsonClassifier(
    DataSources source,
    DateRanges range,
    HashSet<string> required,
    string? dateProperty = null,
    int classifierVersion = 1
) : IFileClassifier
{
    public DataSources Source { get; } = source;
    public int ClassifierVersion => classifierVersion;

    public ImportClassification Classify(FileProbe probe)
    {
        var values = probe.JsonValues;

        if (values == null)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't find valid json.");

        if (!required.IsSubsetOf(values.Keys))
            throw new ClassificationException(
                probe.Filepath,
                Source,
                "Couldn't find required properties."
            );

        var date = DateTime.MinValue;
        if (!string.IsNullOrEmpty(dateProperty) && values.ContainsKey(dateProperty))
        {
            var dateValue = values[dateProperty];
            date = string.IsNullOrEmpty(dateValue) ? date : DateTime.Parse(dateValue);
        }

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetime = range != DateRanges.Multi ? date : DateTime.MinValue,
            Datetype = range,
        };
    }
}
