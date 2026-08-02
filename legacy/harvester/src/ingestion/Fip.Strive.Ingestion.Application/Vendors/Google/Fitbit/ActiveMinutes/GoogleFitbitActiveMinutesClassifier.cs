using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.ActiveMinutes;

public class GoogleFitbitActiveMinutesClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitActiveMinutes,
        DateRanges.Multi,
        "timestamp,light,moderate,very"
    );
