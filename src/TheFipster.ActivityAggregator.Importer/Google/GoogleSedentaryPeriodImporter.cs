using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleSedentaryPeriodImporter()
    : GoogleCsvParser(
        DataSources.FitbitTakeoutSedentaryPeriod,
        DateRanges.Day,
        "start time,end time"
    ),
        IFileClassifier;
