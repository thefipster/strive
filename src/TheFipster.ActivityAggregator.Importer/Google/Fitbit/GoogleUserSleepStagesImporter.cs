using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleUserSleepStagesImporter : IFileClassifier
{
    public DataSources Source => DataSources.FitbitTakeoutUserSleepStages;
    public int ClassifierVersion => 1;
    public int ExtractorVersion => 1;

    private readonly string _header =
        "sleep_id,sleep_stage_id,sleep_stage_type,start_utc_offset,sleep_stage_start,end_utc_offset,sleep_stage_end,data_source,sleep_stage_created,sleep_stage_last_updated";

    public ImportClassification Classify(FileProbe probe)
    {
        var lines = probe.Lines?.Take(1).ToArray();

        if (lines == null || lines.Length == 0)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get any lines.");

        if (_header != lines.First())
            throw new ClassificationException(probe.Filepath, Source, "Couldn't match header.");

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetype = DateRanges.AllTime,
        };
    }
}
