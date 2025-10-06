using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleActiveMinutesImporter()
    : GoogleCsvParser(
        DataSources.FitbitTakeoutActiveMinutes,
        DateRanges.Month,
        "timestamp,light,moderate,very"
    ),
        IFileClassifier;
