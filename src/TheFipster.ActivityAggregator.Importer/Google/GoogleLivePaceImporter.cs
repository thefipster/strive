using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleLivePaceImporter()
    : GoogleCsvParser(
        DataSources.FitbitTakeoutLivePace,
        DateRanges.Month,
        "timestamp,steps,distance millimeters,altitude gain millimeters"
    ),
        IFileClassifier;
