using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;

namespace Fip.Strive.Core.Domain.Extensions
{
    public static class DateExtensions
    {
        public static string GetPath(this DateTime date, DataKind kind) =>
            kind switch
            {
                DataKind.Day => $"{date:yyyy}_{date:MM}_{date:dd}",
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
