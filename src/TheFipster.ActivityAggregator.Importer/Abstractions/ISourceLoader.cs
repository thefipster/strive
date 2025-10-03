using TheFipster.ActivityAggregator.Domain.Models.Scanner;

namespace TheFipster.ActivityAggregator.Importer.Abstractions
{
    public interface ISourceLoader
    {
        Dictionary<string, List<ImportClassification>> Scan(string googleTakeout);

        List<IFileClassifier> GetImporters();
    }
}
