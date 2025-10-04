using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer;

public class Registry : IImporterRegistry
{
    private IReadOnlyList<IFileImporter>? _importers;
    private IReadOnlyList<IFileClassifier>? _classifiers;
    private IReadOnlyList<IFileExtractor>? _extractors;

    public IEnumerable<IFileClassifier> LoadClassifiers()
    {
        if (_classifiers == null)
        {
            var assembly = typeof(Registry).Assembly;

            _classifiers = assembly
                .GetTypes()
                .Where(t =>
                    !t.IsAbstract && !t.IsInterface && typeof(IFileClassifier).IsAssignableFrom(t)
                )
                .Select(t => (IFileClassifier)Activator.CreateInstance(t)!)
                .ToList();
        }

        return _classifiers;
    }

    public IEnumerable<IFileExtractor> LoadExtractors()
    {
        if (_extractors == null)
        {
            var assembly = typeof(Registry).Assembly;

            _extractors = assembly
                .GetTypes()
                .Where(t =>
                    !t.IsAbstract && !t.IsInterface && typeof(IFileExtractor).IsAssignableFrom(t)
                )
                .Select(t => (IFileExtractor)Activator.CreateInstance(t)!)
                .ToList();
        }

        return _extractors;
    }

    public IEnumerable<IFileImporter> LoadImporters()
    {
        if (_importers == null)
        {
            var assembly = typeof(Registry).Assembly;

            _importers = assembly
                .GetTypes()
                .Where(t =>
                    !t.IsAbstract && !t.IsInterface && typeof(IFileImporter).IsAssignableFrom(t)
                )
                .Select(t => (IFileImporter)Activator.CreateInstance(t)!)
                .ToList();
        }

        return _importers;
    }
}
