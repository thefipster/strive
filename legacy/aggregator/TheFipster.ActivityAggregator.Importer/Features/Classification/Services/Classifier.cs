using Microsoft.Extensions.Logging;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Contracts;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Services.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Services;

public class Classifier(IEnumerable<IFileClassifier> classifiers, ILogger<Classifier> logger)
    : IClassifier
{
    public List<ClassificationResult> Classify(FileInfo file, CancellationToken ct)
    {
        var probe = new FileProbe(file.FullName);
        var classifications = new List<ClassificationResult>();

        foreach (var classifier in classifiers)
        {
            var classification = TryClassification(file.FullName, classifier, probe);
            classification.Version = classifier.ClassifierVersion;
            classifications.Add(classification);

            if (ct.IsCancellationRequested)
                break;
        }

        return classifications;
    }

    public IFileClassifier GetClassifier(DataSources source) =>
        classifiers.FirstOrDefault(x => x.Source == source)
        ?? throw new ClassificationException($"Couldn't load classifier {source}");

    private ClassificationResult TryClassification(
        string filepath,
        IFileClassifier importer,
        FileProbe probe
    )
    {
        var result = new ClassificationResult(importer.Source);

        try
        {
            var import = importer.Classify(probe);
            result.Classification = import.ToClassification();
            result.Version = importer.ClassifierVersion;
        }
        catch (ClassificationException e)
        {
            result.Exception = e;
        }
        catch (Exception e)
        {
            result.Exception = e;
            WriteLog(filepath, importer, e);
        }

        return result;
    }

    private void WriteLog(string filepath, IFileClassifier classifier, Exception e)
    {
        logger.LogError(
            e,
            "Unexpected error while classifying file {ImportFile} using classifier {Classifier}.",
            filepath,
            classifier.Source
        );
    }
}
