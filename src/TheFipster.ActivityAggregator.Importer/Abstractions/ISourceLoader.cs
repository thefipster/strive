using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Modules.Abstractions
{
    public interface ISourceLoader
    {
        Dictionary<string, List<ImportClassification>> Scan(string googleTakeout);

        List<IFileClassifier> GetImporters();
    }
}
