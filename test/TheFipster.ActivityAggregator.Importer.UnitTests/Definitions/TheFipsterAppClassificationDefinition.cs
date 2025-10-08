using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.FipsterApp;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Definitions;

public class TheFipsterAppClassificationDefinition
{
    public static IEnumerable<object[]> GetClassificationCases()
    {
        yield return
        [
            new FileClassificationCase(
                typeof(TheFipsterAppWeightImporter),
                Vendors.TheFipsterApp,
                "weights.json",
                "0001-01-01 00:00:00",
                DateRanges.AllTime,
                DataSources.TheFipsterAppWeight
            ),
        ];
    }
}
