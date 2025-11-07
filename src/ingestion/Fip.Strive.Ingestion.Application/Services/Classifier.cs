using System.Text;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Application.Services.Contracts;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Fip.Strive.Ingestion.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Ingestion.Application.Services;

public class Classifier(IEnumerable<IFileClassifier> classifiers, ILogger<Classifier> logger)
    : IClassifier
{
    public IFileClassifier? Find(DataSources source) =>
        classifiers.FirstOrDefault(x => x.Source == source);

    public IEnumerable<IFileClassifier> GetAll() => classifiers;

    public string GetHash()
    {
        var orderedClassifiers = classifiers
            .OrderBy(x => x.Source)
            .Select(x => $"{x.Source}:{x.ClassifierVersion}");

        var hashInput = string.Join("|", orderedClassifiers);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput));

        return Convert.ToHexString(hashBytes);
    }

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
