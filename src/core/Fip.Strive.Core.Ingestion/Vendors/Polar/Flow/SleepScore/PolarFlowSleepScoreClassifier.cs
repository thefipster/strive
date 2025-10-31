using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.SleepScore;

public class PolarFlowSleepScoreClassifier : IFileClassifier
{
    public DataSources Source => DataSources.PolarFlowSleepScore;
    public int ClassifierVersion => 1;

    private readonly HashSet<string> _required =
    [
        "night",
        "sleepScoreResult",
        "sleepScore",
        "sleepTimeOwnTargetScore",
        "sleepTimeRecommendationScore",
    ];

    public ImportClassification Classify(FileProbe probe)
    {
        var values = probe.JsonValues;

        if (values == null)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't find valid json.");

        if (!_required.IsSubsetOf(values.Keys))
            throw new ClassificationException(
                probe.Filepath,
                Source,
                "Couldn't find required properties."
            );

        var date = values["night"];
        if (string.IsNullOrWhiteSpace(date))
            throw new ClassificationException(probe.Filepath, Source, "Couldn't find date value.");

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetime = DateTime.Parse(date),
            Datetype = DateRanges.Multi,
        };
    }
}
