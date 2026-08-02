using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitRespiratoryRateSleepSummarClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitRespiratoryRateSleepSummary,
        DateRanges.Month,
        "timestamp,deep sleep stats - milli breaths per minute,deep sleep stats - standard deviation milli breaths per minute,deep sleep stats - signal to noise,light sleep stats - milli breaths per minute,light sleep stats - standard deviation milli breaths per minute,light sleep stats - signal to noise,rem sleep stats - milli breaths per minute,rem sleep stats - standard deviation milli breaths per minute,rem sleep stats - signal to noise,full sleep stats - milli breaths per minute,full sleep stats - standard deviation milli breaths per minute,full sleep stats - signal to noise"
    );
