using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleHeartrateImporter : IFileClassifier
{
    public DataSources Source => DataSources.FitbitTakeoutHeartrate;
    public int ClassifierVersion => 1;
    public int ExtractorVersion => 1;

    private readonly string _header = "timestamp,beats per minute";

    public ImportClassification Classify(FileProbe probe)
    {
        var lines = probe.Lines?.Take(2).ToArray();

        if (lines == null)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get any lines.");

        if (lines.Length != 2)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get two lines.");

        if (_header != lines.First())
            throw new ClassificationException(probe.Filepath, Source, "Couldn't match header.");

        var cells = lines.Last().Split(",");
        var date = DateTime.Parse(cells[0]);

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetime = date,
            Datetype = probe.Filepath.Contains("daily") ? DateRanges.AllTime : DateRanges.Day,
        };
    }
}
