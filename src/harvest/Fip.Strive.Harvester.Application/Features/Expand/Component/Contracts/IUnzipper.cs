using Fip.Strive.Harvester.Application.Features.Expand.Models;

namespace Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;

public interface IUnzipper
{
    /// <summary>
    /// Extracts the contents of a zip file into the target directory.
    /// </summary>
    /// <param name="zipFilePath">The full path to the zip file.</param>
    /// <param name="overwrite">If files in the output directory should be overwritten.</param>
    /// <returns>
    /// The base path of the extracted files.
    /// </returns>
    string Extract(string zipFilePath, bool overwrite = false);
}
