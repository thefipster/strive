using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Calendar;

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
            Datetype = DateRanges.Multi,
        };
    }
}
