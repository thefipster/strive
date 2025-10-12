using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleCaloriesImporter()
    : GoogleCsvParser(DataSources.GoogleFitbitCalories, DateRanges.Month, "timestamp,calories"),
        IFileClassifier;
