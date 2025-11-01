using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.RespirationSleep;

public class GoogleFitbitRespiratoryRateSleepSummarClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitRespiratoryRateSleepSummary,
        DateRanges.Multi,
        "timestamp,deep sleep stats - milli breaths per minute,deep sleep stats - standard deviation milli breaths per minute,deep sleep stats - signal to noise,light sleep stats - milli breaths per minute,light sleep stats - standard deviation milli breaths per minute,light sleep stats - signal to noise,rem sleep stats - milli breaths per minute,rem sleep stats - standard deviation milli breaths per minute,rem sleep stats - signal to noise,full sleep stats - milli breaths per minute,full sleep stats - standard deviation milli breaths per minute,full sleep stats - signal to noise"
    );
