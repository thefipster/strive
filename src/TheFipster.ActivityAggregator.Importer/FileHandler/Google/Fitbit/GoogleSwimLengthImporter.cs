using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleSwimLengthImporter()
    : GoogleCsvParser(
        DataSources.GoogleFitbitSwimLength,
        DateRanges.Month,
        "timestamp,lap time,stroke count,stroke type"
    ),
        IFileClassifier;
