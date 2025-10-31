using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Altitude;

public class GoogleFitbitAltitudeClassifier()
    : CsvClassifier(DataSources.GoogleFitbitAltitude, DateRanges.Multi, "timestamp,gain");
