using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Ingestion.Options;

public static class ClassificationOptions
{
    public const string All = "All";
    public const string Classified = "Classified";
    public const string Unclassified = "Unclassified";

    public static List<string> AllItems =>
        ((string[])[All, Classified, Unclassified]).Concat(EnumItems).ToList();

    public static List<string> EnumItems => Enum.GetNames(typeof(DataSources)).ToList();
}
