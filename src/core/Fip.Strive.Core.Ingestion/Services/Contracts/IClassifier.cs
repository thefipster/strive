using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Services.Contracts;

public interface IClassifier
{
    List<ClassificationResult> Classify(string filepath, CancellationToken ct);

    IFileClassifier? Find(DataSources source);

    IEnumerable<IFileClassifier> GetAll();

    string GetHash();
}
