using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Components;

public abstract class CsvClassifier(
    DataSources source,
    DateRanges range,
    string header,
    string delimiter = ",",
    int? dateColumn = null,
    int classifierVersion = 1
) : IFileClassifier
{
    public DataSources Source { get; } = source;
    public int ClassifierVersion => classifierVersion;

    public ImportClassification Classify(FileProbe probe)
    {
        var lines = probe.Lines?.Take(2).ToArray();

        if (lines == null)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get any lines.");

        if (lines.Length != 2)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get two lines.");

        if (header != lines.First())
            throw new ClassificationException(probe.Filepath, Source, "Couldn't match header.");

        var date = DateTime.MinValue;
        if (dateColumn.HasValue)
            date = DateHelper.GetDateFromCsvLine(lines.Last(), delimiter, dateColumn.Value);

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetime = range != DateRanges.Multi ? date : DateTime.MinValue,
            Datetype = range,
        };
    }
}
