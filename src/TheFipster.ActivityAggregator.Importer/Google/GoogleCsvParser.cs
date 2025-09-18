using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleCsvParser(DataSources source, DateRanges range, string header)
{
    public DataSources Source { get; } = source;

    public ImportClassification Classify(FileProbe probe)
    {
        var lines = probe.GetLines().Take(2).ToArray();
        if (lines.Length != 2)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get two lines.");

        if (header != lines.First())
            throw new ClassificationException(probe.Filepath, Source, "Couldn't match header.");

        var date = DateHelper.GetDateFromCsvLine(lines.Last(), ",", 0);

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetime = date,
            Datetype = range,
        };
    }
}
