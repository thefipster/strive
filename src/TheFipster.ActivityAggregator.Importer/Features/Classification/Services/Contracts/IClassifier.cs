using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Services.Contracts;

public interface IClassifier
{
    List<ClassificationResult> Classify(FileInfo file, CancellationToken ct);

    IFileClassifier GetClassifier(DataSources source);
}
