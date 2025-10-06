namespace TheFipster.ActivityAggregator.Importer.Abstractions;

public interface IImporterRegistry
{
    IEnumerable<IFileClassifier> LoadClassifiers();

    IEnumerable<IFileExtractor> LoadExtractors();
}
