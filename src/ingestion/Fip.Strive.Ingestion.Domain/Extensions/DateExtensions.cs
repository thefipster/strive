using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Components;

namespace Fip.Strive.Core.Ingestion.Domain.Extensions
{
    public static class DateExtensions
    {
        public static string GetPath(this DateTime date, DataKind kind) =>
            kind switch
            {
                DataKind.Day => $"{date:yyyy}\\{date:MM}\\{date:dd}",
                DataKind.Session => $"{date:yyyy}\\{date:MM}\\{date:dd}\\{date:HH_mm}",
                _ => string.Empty,
            };

        public static string ToRangeString(this DateTime date, DataKind kind) =>
            kind switch
            {
                DataKind.Day => date.ToString(DateHelper.DayFormat),
                DataKind.Session => date.ToString(DateHelper.FsMillisecondFormat),
                _ => string.Empty,
            };
    }
}
