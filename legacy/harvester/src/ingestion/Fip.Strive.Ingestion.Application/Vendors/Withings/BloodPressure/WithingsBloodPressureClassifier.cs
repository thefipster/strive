using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Withings.BloodPressure;

public class WithingsBloodPressureClassifier()
    : CsvClassifier(
        DataSources.WithingsBloodPressure,
        DateRanges.Multi,
        "Date,\"Heart rate\",Systolic,Diastolic,Comments",
        ",",
        0
    );
