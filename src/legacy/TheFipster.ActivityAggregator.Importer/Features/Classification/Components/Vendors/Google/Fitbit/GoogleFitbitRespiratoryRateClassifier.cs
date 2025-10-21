using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitRespiratoryRateClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitRespiratoryRate,
        DateRanges.AllTime,
        "timestamp,breaths per minute"
    );
