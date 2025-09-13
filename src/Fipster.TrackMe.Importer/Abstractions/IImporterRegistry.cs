namespace Fipster.TrackMe.Importer.Modules.Abstractions;

public interface IImporterRegistry
{
    IEnumerable<IFileClassifier> LoadClassifiers();

    IEnumerable<IFileExtractor> LoadExtractors();

    IEnumerable<IFileImporter> LoadImporters();
}
