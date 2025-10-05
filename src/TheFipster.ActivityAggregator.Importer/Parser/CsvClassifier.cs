using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Parser;

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
            Datetime = range != DateRanges.AllTime ? date : DateTime.MinValue,
            Datetype = range,
        };
    }
}
