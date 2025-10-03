using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleCaloriesImporter()
    : GoogleCsvParser(DataSources.FitbitTakeoutCalories, DateRanges.Month, "timestamp,calories"),
        IFileClassifier;
