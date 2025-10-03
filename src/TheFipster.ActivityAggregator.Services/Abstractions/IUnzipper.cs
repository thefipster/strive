using TheFipster.ActivityAggregator.Domain.Models.Components;

namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface IUnzipper
{
    DirectoryStats Extract(string zipFilePath, string destinationDirectory, bool overwrite = false);
}
