using TheFipster.ActivityAggregator.Domain.Models;
using LiteDB;

namespace TheFipster.ActivityAggregator.Storage.Lite;

public class LiteDbConfig
{
    public const string ConfigSectionName = "LiteDb";

    public string IndexFile { get; set; }
}
