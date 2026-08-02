using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitHeartrateVariabilityClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitHeartrateVariability,
        DateRanges.Month,
        "timestamp,root mean square of successive differences milliseconds,standard deviation milliseconds"
    );
