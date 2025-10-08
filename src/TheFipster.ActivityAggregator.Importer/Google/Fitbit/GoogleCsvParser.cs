using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Tools;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleCsvParser(
    DataSources source,
    DateRanges range,
    string header,
    int classifierVersion = 1,
    int extractorVersion = 1
)
{
    public DataSources Source { get; } = source;
    public int ClassifierVersion => classifierVersion;
    public int ExtractorVersion => extractorVersion;

    public ImportClassification Classify(FileProbe probe)
    {
        var lines = probe.Lines?.Take(2).ToArray();

        if (lines == null)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get any lines.");

        if (lines.Length != 2)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get two lines.");

        if (header != lines.First())
            throw new ClassificationException(probe.Filepath, Source, "Couldn't match header.");

        var date = DateHelper.GetDateFromCsvLine(lines.Last(), ",", 0);

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetime = range != DateRanges.AllTime ? date : DateTime.MinValue,
            Datetype = range,
        };
    }
}
