using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Swim;

public class GoogleFitbitSwimLengthClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitSwimLength,
        DateRanges.Multi,
        "timestamp,lap time,stroke count,stroke type"
    );
