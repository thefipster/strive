using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Withings;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Definitions;

public class WithingsClassificationDefinitions
{
    public static IEnumerable<object[]> GetClassificationCases()
    {
        yield return
        [
            new FileClassificationCase(
                typeof(WithingsBloodPressureClassifier),
                Vendors.Withings,
                "bp.csv",
                "0001-01-01 00:00:00",
                DateRanges.AllTime,
                DataSources.WithingsBloodPressure
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(WithingsSignalClassifier),
                Vendors.Withings,
                "signal.csv",
                "0001-01-01 00:00:00",
                DateRanges.AllTime,
                DataSources.WithingsSignal
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(WithingsOtherClassifier),
                Vendors.Withings,
                "other.csv",
                "0001-01-01 00:00:00",
                DateRanges.AllTime,
                DataSources.WithingsOther
            ),
        ];
    }
}
