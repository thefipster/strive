using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;

namespace TheFipster.ActivityAggregator.Storage.Lite.Abstraction;

public interface IDayWriter
{
    void SetExtraction(FileExtraction extraction);
}
