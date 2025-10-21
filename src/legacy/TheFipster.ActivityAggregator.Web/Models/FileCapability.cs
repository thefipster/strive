using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Web.Models;

public class FileCapability
{
    public DataSources Source { get; set; }
    public int? ClassifierVersion { get; set; }
    public int? ExtractorVersion { get; set; }
}
