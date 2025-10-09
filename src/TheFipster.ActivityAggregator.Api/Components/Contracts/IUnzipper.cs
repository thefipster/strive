using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Components.Contracts;

public interface IUnzipper
{
    DirectoryStats Extract(string zipFilePath, bool overwrite = false);
}
