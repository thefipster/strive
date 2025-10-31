using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.ActiveMinutes;

public class GoogleFitbitActiveMinutesClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitActiveMinutes,
        DateRanges.Multi,
        "timestamp,light,moderate,very"
    );
