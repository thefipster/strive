using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Services.Contracts;

public interface IClassifier
{
    List<ClassificationResult> Classify(string filepath, CancellationToken ct);

    IFileClassifier? Find(DataSources source);

    IEnumerable<IFileClassifier> GetAll();

    string GetHash();
}
