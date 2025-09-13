using Fipster.TrackMe.Importer.Cli.Sources;
using Fipster.TrackMe.Importer.Modules.Abstractions;
using Fipster.TrackMe.Importer.Modules.Generic;
using Fipster.TrackMe.Importer.Modules.Google;
using Fipster.TrackMe.Importer.Modules.RunGps;
using Fipster.TrackMe.Importer.Polar;
using Fipster.TrackMe.Importer.RunGps;

namespace Fipster.TrackMe.Cli
{
    internal class Setup
    {
        internal Dictionary<ISourceLoader, List<string>> GetSources(string archive)
        {
            var sources = new Dictionary<ISourceLoader, List<string>>
            {
                { new PolarTakeoutSource(), [@"E:\polar\import\polar_takeout"] },
                { new PolarProTrainerSource(), [@"E:\polar\import\polar_protrainer"] },
                { new MyManiaSource(), [@"E:\polar\import\my_collection"] },
                // { new ApiLeacherSource(), [@"E:\polar\import\my_conversions"] },
                { new GoogleTakeoutSource(), [@"E:\polar\import\fitbit_takeout"] },
            };
            return sources;
        }

        internal List<IFileExtractor> GetExtractors()
        {
            return
            [
                new GoogleStepsImporter(),
                new KmlImporter(),
                new GpxImporter(),
                new PolarProTrainerHrmImporter(),
                new RunGpsCsvImporter(),
                new RunGpsCsv2Importer(),
                new PolarTakeoutTrainingImporter(),
                new PolarTakeoutActivityImporter(),
                new PolarTakeoutPpiImporter(),
                new PolarTakeoutSleepResultImporter(),
                new PolarTakeoutSleepScoreImporter(),
                new PolarTakeoutCalendarItemsImporter(),
            ];
        }
    }
}
