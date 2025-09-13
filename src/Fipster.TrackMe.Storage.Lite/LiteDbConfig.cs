using Fipster.TrackMe.Domain.Models;
using LiteDB;

namespace Fipster.TrackMe.Storage.Lite;

public class LiteDbConfig
{
    public const string ConfigSectionName = "LiteDb";

    public string IndexFile { get; set; }
}
