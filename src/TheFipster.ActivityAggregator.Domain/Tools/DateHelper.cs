namespace TheFipster.ActivityAggregator.Domain.Tools;

public static class DateHelper
{
    public static DateTime GetDateFromMyCollectionDirectory(DirectoryInfo? directory)
    {
        if (directory is not { Exists: true })
            throw new DirectoryNotFoundException(
                $"The directory {directory?.FullName} could not be found."
            );

        var dirName = directory.Name;
        var parts = dirName.Split(" - ");
        var dateString = parts[0].Replace(".", "-");
        var timeString = parts[1];
        return DateTime.Parse(
            $"{dateString} {timeString.Substring(0, 2)}:{timeString.Substring(2, 2)}"
        );
    }

    public static DateTime GetDateFromCsvLine(string line, string separator, int index)
    {
        var cells = line.Split(separator);
        return DateTime.Parse(cells[index]);
    }
}
