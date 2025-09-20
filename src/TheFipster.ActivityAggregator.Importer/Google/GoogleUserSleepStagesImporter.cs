using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleUserSleepStagesImporter : IFileClassifier
{
    public DataSources Source => DataSources.FitbitTakeoutUserSleepStages;

    private readonly string header =
        "sleep_id,sleep_stage_id,sleep_stage_type,start_utc_offset,sleep_stage_start,end_utc_offset,sleep_stage_end,data_source,sleep_stage_created,sleep_stage_last_updated";

    public ImportClassification Classify(FileProbe probe)
    {
        var lines = probe.Lines?.Take(1).ToArray();

        if (lines == null || lines.Length == 0)
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
