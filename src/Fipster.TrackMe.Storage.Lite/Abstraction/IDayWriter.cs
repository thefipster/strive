using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Models;

namespace Fipster.TrackMe.Storage.Lite.Abstraction;

public interface IDayWriter
{
    void SetExtraction(FileExtraction extraction);
}
