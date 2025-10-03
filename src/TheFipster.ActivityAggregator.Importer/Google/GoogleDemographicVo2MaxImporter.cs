using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleDemographicVo2MaxImporter : IFileClassifier
{
    public DataSources Source => DataSources.FitbitTakeoutDemographicVo2Max;
    public int ClassifierVersion => 1;
    public int ExtractorVersion => 1;

    private readonly string header = "timestamp,demographic vo2max";

    public ImportClassification Classify(FileProbe probe)
    {
        var lines = probe.Lines?.Take(1).ToArray();
        if (lines == null || lines.Length != 1)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get any lines.");

        if (header != lines.First())
            throw new ClassificationException(probe.Filepath, Source, "Couldn't match header.");

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetype = DateRanges.AllTime,
        };
    }
}
