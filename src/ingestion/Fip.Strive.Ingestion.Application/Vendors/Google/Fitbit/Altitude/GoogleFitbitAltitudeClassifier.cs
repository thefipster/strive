using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.Altitude;

public class GoogleFitbitAltitudeClassifier()
    : CsvClassifier(DataSources.GoogleFitbitAltitude, DateRanges.Multi, "timestamp,gain");
