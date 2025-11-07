using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.Location;

public class GoogleFitbitGpsLocationClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitGpsLocation,
        DateRanges.Day,
        "timestamp,latitude,longitude,altitude"
    );
