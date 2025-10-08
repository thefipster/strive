using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IClassifier
{
    List<ClassificationResult> Classify(FileInfo file, CancellationToken ct);

    IFileClassifier GetClassifier(DataSources source);
}
