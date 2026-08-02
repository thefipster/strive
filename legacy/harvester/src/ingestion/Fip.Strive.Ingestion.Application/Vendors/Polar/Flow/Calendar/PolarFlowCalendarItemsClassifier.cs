using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Calendar;

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
