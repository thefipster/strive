using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Models;

namespace Fipster.TrackMe.Importer.Modules.Abstractions
{
    public interface IFileExtractor
    {
        DataSources Source { get; }

        List<FileExtraction> Extract(ArchiveIndex file);
    }
}
