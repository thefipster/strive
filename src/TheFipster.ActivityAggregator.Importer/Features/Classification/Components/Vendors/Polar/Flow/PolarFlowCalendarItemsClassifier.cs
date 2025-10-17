using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Polar.Flow;

public class PolarFlowCalendarItemsClassifier : IFileClassifier
{
    public DataSources Source => DataSources.PolarFlowCalendarItems;
    public int ClassifierVersion => 1;

    private readonly HashSet<string> _required =
    [
        "exportVersion",
        "perceivedRecovery",
        "physicalInformations",
    ];

    public ImportClassification Classify(FileProbe probe)
    {
        var props = probe.JsonTags;

        if (props == null)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't find valid json.");

        if (!_required.IsSubsetOf(props))
            throw new ClassificationException(
                probe.Filepath,
                Source,
                "Couldn't find required properties."
            );

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetype = DateRanges.AllTime,
        };
    }
}
