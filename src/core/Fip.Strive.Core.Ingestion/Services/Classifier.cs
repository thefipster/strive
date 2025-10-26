using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;
using Fip.Strive.Core.Ingestion.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Core.Ingestion.Services;

public class Classifier(IEnumerable<IFileClassifier> classifiers, ILogger<Classifier> logger)
    : IClassifier
{
    public List<ClassificationResult> Classify(string filepath, CancellationToken ct)
    {
        var probe = new FileProbe(filepath);
        var classifications = new List<ClassificationResult>();

        foreach (var classifier in classifiers)
        {
            var classification = TryClassification(filepath, classifier, probe);
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
