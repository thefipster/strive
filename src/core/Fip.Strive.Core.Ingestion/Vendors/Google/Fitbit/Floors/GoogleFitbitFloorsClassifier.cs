using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Floors;

public class GoogleFitbitFloorsClassifier()
    : CsvClassifier(DataSources.GoogleFitbitFloors, DateRanges.Multi, "timestamp,floors");
