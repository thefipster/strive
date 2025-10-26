using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.TheFipsterApp.Bodyweight;

public class TheFipsterAppWeightClassifier()
    : JsonClassifier(
        DataSources.TheFipsterAppWeight,
        DateRanges.AllTime,
        ["Weight", "Reason", "Date"],
        "Date"
    );
