using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Contracts
{
    public interface IFileClassifier
    {
        int ClassifierVersion { get; }

        DataSources Source { get; }

        ImportClassification Classify(FileProbe probe);
    }
}
