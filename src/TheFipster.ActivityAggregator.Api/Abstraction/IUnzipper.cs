using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IUnzipper
{
    DirectoryStats Extract(string zipFilePath, string destinationDirectory, bool overwrite = false);
}
