namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface IUnzipper
{
    void Extract(string zipFilePath, string destinationDirectory, bool overwrite = false);
}
