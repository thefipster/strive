using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.Swim;

public class GoogleFitbitSwimLengthClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitSwimLength,
        DateRanges.Multi,
        "timestamp,lap time,stroke count,stroke type"
    );
