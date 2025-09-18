using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleUserSleepImporter : IFileClassifier
{
    public DataSources Source => DataSources.FitbitTakeoutUserSleep;

    private readonly string header =
        "sleep_id,sleep_type,minutes_in_sleep_period,minutes_after_wake_up,minutes_to_fall_asleep,minutes_asleep,minutes_awake,minutes_longest_awakening,minutes_to_persistent_sleep,start_utc_offset,sleep_start,end_utc_offset,sleep_end,data_source,sleep_created,sleep_last_updated";

    public ImportClassification Classify(FileProbe probe)
    {
        var lines = probe.GetLines().Take(1).ToArray();
        if (lines.Length == 0)
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
