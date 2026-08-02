using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.TheFipsterApp.Bodyweight;

public class TheFipsterAppWeightClassifier()
    : JsonClassifier(
        DataSources.TheFipsterAppWeight,
        DateRanges.Multi,
        ["Weight", "Reason", "Date"]
    );
