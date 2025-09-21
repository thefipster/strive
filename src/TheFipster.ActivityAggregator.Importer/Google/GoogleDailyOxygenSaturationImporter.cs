using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleDailyOxygenSaturationImporter()
    : GoogleCsvParser(
        DataSources.FitbitTakeoutDailyOxygenSaturation,
        DateRanges.Month,
        "timestamp,average percentage,lower bound percentage,upper bound percentage,baseline percentage,standard deviation percentage"
    ),
        IFileClassifier;
