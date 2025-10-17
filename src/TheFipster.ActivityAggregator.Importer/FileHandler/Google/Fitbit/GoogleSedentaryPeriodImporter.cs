using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleSedentaryPeriodImporter()
    : GoogleCsvParser(
        DataSources.GoogleFitbitSedentaryPeriod,
        DateRanges.Day,
        "start time,end time"
    ),
        IFileClassifier;
