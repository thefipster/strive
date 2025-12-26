using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.SleepResult;

public class PolarFlowSleepResultClassifier : IFileClassifier
{
    public DataSources Source => DataSources.PolarFlowSleepResult;
    public int ClassifierVersion => 1;

    private readonly HashSet<string> _required =
    [
        "night",
        "evaluation",
        "sleepType",
        "sleepSpan",
        "asleepDuration",
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

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetype = DateRanges.Multi,
        };
    }
}
