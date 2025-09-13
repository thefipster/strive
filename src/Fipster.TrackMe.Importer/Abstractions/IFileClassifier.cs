using Fipster.TrackMe.Domain;

namespace Fipster.TrackMe.Importer.Modules.Abstractions
{
    public interface IFileClassifier
    {
        DataSources Source { get; }

        ImportClassification? Classify(string filepath);
    }
}
