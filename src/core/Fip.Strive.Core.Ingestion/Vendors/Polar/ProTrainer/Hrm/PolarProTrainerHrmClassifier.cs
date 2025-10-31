using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.ProTrainer.Hrm
{
    public class PolarProTrainerHrmClassifier : IFileClassifier
    {
        public DataSources Source => DataSources.PolarProTrainerHrm;
        public int ClassifierVersion => 1;

        public ImportClassification Classify(FileProbe probe)
        {
            var text = probe.Text;

            if (string.IsNullOrEmpty(text))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find any text."
                );

            if (text.Length < 8 || text.Substring(0, 8) != "[Params]")
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find params section."
                );

            var lines = text.Split('\n');

            var dateLine = lines.First(x => x.Substring(0, 4) == "Date");
            var dateParts = dateLine.Split("=");

            var timeLine = lines.First(x => x.Substring(0, 9) == "StartTime");
            var timeParts = timeLine.Split("=");

            var datetime = GetDate(dateParts[1], timeParts[1]);

            return new ImportClassification
            {
                Filepath = probe.Filepath,
                Source = Source,
                Datetime = datetime,
                Datetype = DateRanges.Session,
            };
        }

        private static DateTime GetDate(string dateValue, string startTimeValue)
        {
            var year = int.Parse(dateValue.Substring(0, 4));
            var month = int.Parse(dateValue.Substring(4, 2));
            var day = int.Parse(dateValue.Substring(6, 2));

            var splitParts = startTimeValue.Split('.');
            var timeParts = splitParts[0].Split(":");

            var hour = int.Parse(timeParts[0]);
            var minute = int.Parse(timeParts[1]);
            var second = int.Parse(timeParts[2]);

            var date = new DateTime(year, month, day, hour, minute, second);
            return date;
        }
    }
}
