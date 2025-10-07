using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleUserExercisesImporter : IFileClassifier
{
    public DataSources Source => DataSources.FitbitTakeoutUserExercises;
    public int ClassifierVersion => 1;
    public int ExtractorVersion => 1;

    private readonly string _header =
        "exercise_id,exercise_start,exercise_end,utc_offset,exercise_created,exercise_last_updated,activity_name,log_type,pool_length,pool_length_unit,intervals,distance_units,tracker_total_calories,tracker_total_steps,tracker_total_distance_mm,tracker_total_altitude_mm,tracker_avg_heart_rate,tracker_peak_heart_rate,tracker_avg_pace_mm_per_second,tracker_avg_speed_mm_per_second,tracker_peak_speed_mm_per_second,tracker_auto_stride_run_mm,tracker_auto_stride_walk_mm,tracker_swim_lengths,tracker_pool_length,tracker_pool_length_unit,tracker_cardio_load,manually_logged_total_calories,manually_logged_total_steps,manually_logged_total_distance_mm,manually_logged_pool_length,manually_logged_pool_length_unit,events";

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
