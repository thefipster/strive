using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Withings.BloodPressure;

public class WithingsBloodPressureClassifier()
    : CsvClassifier(
        DataSources.WithingsBloodPressure,
        DateRanges.AllTime,
        "Date,\"Heart rate\",Systolic,Diastolic,Comments",
        ",",
        0
    );
