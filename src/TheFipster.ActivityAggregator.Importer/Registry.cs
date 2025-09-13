using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer;

public class Registry : IImporterRegistry
{
    private IReadOnlyList<IFileImporter>? importers;
    private IReadOnlyList<IFileClassifier>? classifiers;
    private IReadOnlyList<IFileExtractor>? extractors;

    public IEnumerable<IFileClassifier> LoadClassifiers()
    {
        if (classifiers == null)
        {
            var assembly = typeof(Registry).Assembly;

            classifiers = assembly
                .GetTypes()
                .Where(t =>
                    !t.IsAbstract && !t.IsInterface && typeof(IFileClassifier).IsAssignableFrom(t)
                )
                .Select(t => (IFileClassifier)Activator.CreateInstance(t)!)
                .ToList();
        }

        return classifiers;
    }

    public IEnumerable<IFileExtractor> LoadExtractors()
    {
        if (extractors == null)
        {
            var assembly = typeof(Registry).Assembly;

            extractors = assembly
                .GetTypes()
                .Where(t =>
                    !t.IsAbstract && !t.IsInterface && typeof(IFileExtractor).IsAssignableFrom(t)
                )
                .Select(t => (IFileExtractor)Activator.CreateInstance(t)!)
                .ToList();
        }

        return extractors;
    }

    public IEnumerable<IFileImporter> LoadImporters()
    {
        if (importers == null)
        {
            var assembly = typeof(Registry).Assembly;

            importers = assembly
                .GetTypes()
                .Where(t =>
                    !t.IsAbstract && !t.IsInterface && typeof(IFileImporter).IsAssignableFrom(t)
                )
                .Select(t => (IFileImporter)Activator.CreateInstance(t)!)
                .ToList();
        }

        return importers;
    }
}
