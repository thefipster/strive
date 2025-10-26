using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;

namespace Fip.Strive.Core.Ingestion.Contracts
{
    public interface IFileClassifier
    {
        int ClassifierVersion { get; }

        DataSources Source { get; }

        ImportClassification Classify(FileProbe probe);
    }
}
