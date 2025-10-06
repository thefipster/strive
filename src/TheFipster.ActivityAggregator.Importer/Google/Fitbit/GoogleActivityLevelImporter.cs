using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleActivityLevelImporter()
    : GoogleCsvParser(DataSources.FitbitTakeoutActivityLevel, DateRanges.Month, "timestamp,level"),
        IFileClassifier;
