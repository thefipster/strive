using TheFipster.ActivityAggregator.Importer.Google;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Importer.Modules.Google;

namespace Fipster.TrackMe.Importer.Cli.Sources
{
    public class GoogleTakeoutSource() : ISourceLoader
    {
        private readonly List<IFileClassifier> importers = new()
        {
            new GoogleActiveMinutesImporter(),
            new GoogleActiveZoneMinutesImporter(),
            new GoogleActivityLevelImporter(),
            new GoogleAltitudeImporter(),
            new GoogleCaloriesImporter(),
            new GoogleCaloriesHeartrateZoneImporter(),
            new GoogleDailyHeartrateVariabilityImporter(),
            new GoogleDailyOxygenSaturationImporter(),
            new GoogleDemographicVo2MaxImporter(),
            new GoogleDistanceImporter(),
            new GoogleFloorsImporter(),
            new GoogleGpsLocationImporter(),
            new GoogleHeartrateImporter(),
            new GoogleHeartrateNotificationImporter(),
            new GoogleHeartrateVariabilityImporter(),
            new GoogleLivePaceImporter(),
            new GoogleOxygenSaturationImporter(),
            new GoogleRespiratoryRateImporter(),
            new GoogleRespiratoryRateSleepSummaryImporter(),
            new GoogleRunVo2MaxImporter(),
            new GoogleSedentaryPeriodImporter(),
            new GoogleStepsImporter(),
            new GoogleSwimLengthImporter(),
            new GoogleUserExercisesImporter(),
            new GoogleUserSleepImporter(),
            new GoogleUserSleepScoreImporter(),
            new GoogleUserSleepStagesImporter(),
            new GoogleWeightImporter(),
        };

        public List<IFileClassifier> GetImporters() => importers;

        public Dictionary<string, List<ImportClassification>> Scan(string googleTakeout)
        {
            var result = new Dictionary<string, List<ImportClassification>>();
            var dirs = Directory.GetDirectories(googleTakeout);
            var userPath = Path.Combine(googleTakeout, dirs.First());
            var dataPaths = new Dictionary<string, List<string>>
            {
                {
                    Path.Combine(userPath, "Google Data", "Health Fitness Data"),
                    new List<string>
                    {
                        "UserExercises_*",
                        "UserSleeps_*",
                        "UserSleepScores_*",
                        "UserSleepStages_*",
                    }
                },
                {
                    Path.Combine(userPath, "Google Data", "Physical Activity"),
                    new List<string> { "*.*" }
                },
            };

            var blacklistExt = new List<string> { ".txt" };

            foreach (var dataPath in dataPaths)
            {
                var files = new List<string>();
                foreach (var pattern in dataPath.Value)
                    files.AddRange(Directory.GetFiles(dataPath.Key, pattern));

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);

                    if (blacklistExt.Any(x => x == fileInfo.Extension))
                        continue;

                    var results = Classify(file);
                    result.Add(file, results);
                }
            }

            return result;
        }

        private List<ImportClassification> Classify(string file)
        {
            var results = new List<ImportClassification>();

            foreach (var importer in importers)
            {
                var result = importer.Classify(file);
                if (result == null)
                    continue;

                results.Add(result);
            }

            return results;
        }
    }
}
