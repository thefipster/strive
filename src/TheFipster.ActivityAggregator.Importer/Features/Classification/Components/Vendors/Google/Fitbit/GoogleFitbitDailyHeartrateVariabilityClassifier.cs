using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitDailyHeartrateVariabilityClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitDailyHeartrateVariability,
        DateRanges.Month,
        "timestamp,average heart rate variability milliseconds,non rem heart rate beats per minute,entropy,deep sleep root mean square of successive differences milliseconds"
    );
