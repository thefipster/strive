using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

public interface IImporterRegistry
{
    IEnumerable<IFileClassifier> LoadClassifiers();

    IEnumerable<IFileExtractor> LoadExtractors();

    IEnumerable<IFileImporter> LoadImporters();
}
