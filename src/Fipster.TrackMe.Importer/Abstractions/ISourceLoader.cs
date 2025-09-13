namespace Fipster.TrackMe.Importer.Modules.Abstractions
{
    public interface ISourceLoader
    {
        Dictionary<string, List<ImportClassification>> Scan(string googleTakeout);

        List<IFileClassifier> GetImporters();
    }
}
