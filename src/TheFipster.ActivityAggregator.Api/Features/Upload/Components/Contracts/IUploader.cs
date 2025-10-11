using TheFipster.ActivityAggregator.Api.Features.Upload.Models;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Components.Contracts;

public interface IUploader
{
    /// <summary>
    /// Writes chunks to disc, if file is incomplete the return value will be null. After the last chunk the file will be finalized and the filepath will be returned.
    /// </summary>
    /// <param name="request">Request to upload a chunk</param>
    /// <returns>Filepath of the final file when finished, until then null.</returns>
    Task<string?> EnsureChunkAsync(UploadChunkRequest request);
}
