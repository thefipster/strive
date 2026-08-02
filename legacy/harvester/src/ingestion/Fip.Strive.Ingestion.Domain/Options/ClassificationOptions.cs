using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Domain.Options;

public static class ClassificationOptions
{
    public const string All = "All";
    public const string Classified = "Classified";
    public const string Unclassified = "Unclassified";

    public static List<string> AllItems =>
        ((string[])[All, Classified, Unclassified]).Concat(EnumItems).ToList();

    public static List<string> EnumItems => Enum.GetNames(typeof(DataSources)).ToList();
}
