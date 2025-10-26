using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Location;

public class GoogleFitbitGpsLocationClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitGpsLocation,
        DateRanges.Day,
        "timestamp,latitude,longitude,altitude"
    );
