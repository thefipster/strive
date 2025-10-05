using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Google;
using TheFipster.ActivityAggregator.Importer.Gpsies;
using TheFipster.ActivityAggregator.Importer.Polar;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Definitions;

public static class PolarProTrainerClassificationDefinitions
{
    public static IEnumerable<object[]> GetClassificationCases()
    {
        yield return
        [
            new FileClassificationCase(
                typeof(PolarProTrainerHrmImporter),
                Vendors.PolarProTrainer,
                "09041005.hrm",
                "2009-04-10 20:34:46",
                DateRanges.Time,
                DataSources.PolarProTrainerHrm
            ),
        ];
    }
}
