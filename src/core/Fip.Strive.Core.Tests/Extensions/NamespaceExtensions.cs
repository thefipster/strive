using ArchUnitNET.Domain;

namespace Fip.Strive.Core.Tests.Extensions;

public static class NamespaceExtensions
{
    public static string GetNamespaceOfFeature(this Namespace fullNamespace, string root)
    {
        var full = fullNamespace.FullName;
        
        if (!full.StartsWith(root))
            throw new ArgumentException($"{root} is not part of {full}");
        
        if (full == root)
            throw new ArgumentException($"{root} is not a feature");
        
        var rootSplit = root.Split(".");
        var fullSplit = full.Split(".");
        var feature = fullSplit[rootSplit.Length];
        
        return root + "." + feature;
    }
}