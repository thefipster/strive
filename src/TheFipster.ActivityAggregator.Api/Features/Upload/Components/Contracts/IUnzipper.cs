using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Components.Contracts;

public interface IUnzipper
{
    /// <summary>
    /// Extracts the contents of a zip file into the target directory.
    /// </summary>
    /// <param name="zipFilePath">The full path to the zip file.</param>
    /// <param name="overwrite">If files in the output directory should be overwritten.</param>
    DirectoryStats Extract(string zipFilePath, bool overwrite = false);
}
