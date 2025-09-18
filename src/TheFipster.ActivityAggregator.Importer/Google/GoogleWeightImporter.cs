using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleWeightImporter : IFileClassifier
{
    public DataSources Source => DataSources.FitbitTakeoutWeight;

    private string Header = "timestamp,weight grams";

    public ImportClassification Classify(FileProbe probe)
    {
        var lines = probe.GetLines().Take(1).ToArray();
        if (lines.Length != 1)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get any lines.");

        if (Header != lines.First())
            throw new ClassificationException(probe.Filepath, Source, "Couldn't match header.");

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetype = DateRanges.AllTime,
        };
    }
}
