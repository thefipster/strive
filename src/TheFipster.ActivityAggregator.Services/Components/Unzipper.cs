using System.IO.Compression;
using TheFipster.ActivityAggregator.Services.Abstractions;

namespace TheFipster.ActivityAggregator.Services.Components;

public class Unzipper : IUnzipper
{
    /// <summary>
    /// Extracts the contents of a zip file into the target directory.
    /// </summary>
    /// <param name="zipFilePath">The full path to the zip file.</param>
    /// <param name="destinationDirectory">The directory to extract into.</param>
    /// <param name="overwrite">If files in the output directory should be overwritten.</param>
    public void Extract(string zipFilePath, string destinationDirectory, bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(zipFilePath))
            throw new ArgumentException("Zip file path must not be empty.", nameof(zipFilePath));

        if (string.IsNullOrWhiteSpace(destinationDirectory))
            throw new ArgumentException(
                "Destination directory must not be empty.",
                nameof(destinationDirectory)
            );

        if (!File.Exists(zipFilePath))
            throw new FileNotFoundException("The zip file was not found.", zipFilePath);

        try
        {
            Directory.CreateDirectory(destinationDirectory); // ensures target directory exists
        }
        catch (Exception e)
        {
            throw new ArgumentException(
                "Destination directory could not be created.",
                nameof(destinationDirectory),
                e
            );
        }

        ZipFile.ExtractToDirectory(zipFilePath, destinationDirectory, overwrite);
    }
}
